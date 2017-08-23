using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using FirebirdSql.Data.FirebirdClient;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class FirebirdDatabaseModelFactory : IDatabaseModelFactory
    {
        FbConnection _connection;
        TableSelectionSet _tableSelectionSet;
        DatabaseModel _databaseModel;
        Dictionary<string, DatabaseTable> _tables;
        Dictionary<string, DatabaseColumn> _tableColumns;

        static string TableKey(DatabaseTable table) => TableKey(table.Name, table.Schema);
        static string TableKey(string name, string schema) => $"{name}";
        static string ColumnKey(DatabaseTable table, string columnName) => $"{TableKey(table)}.{columnName}";


        public FirebirdDatabaseModelFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();
            Logger = loggerFactory.CreateCommandsLogger();
        }

        public virtual ILogger Logger { get; }

        void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, DatabaseTable>();
            _tableColumns = new Dictionary<string, DatabaseColumn>(StringComparer.OrdinalIgnoreCase);
        }

        public DatabaseModel Create(string connectionString, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            using (var connection = new FbConnection(connectionString))
            {
                return Create(connection, tables, schemas);
            }
        }

        public DatabaseModel Create(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            return Create(connection, new TableSelectionSet(tables, schemas));
        }

        public DatabaseModel Create(DbConnection connection, TableSelectionSet tableSelectionSet)
        {
            ResetState();

            _connection = (FbConnection)connection;

            var connectionStartedOpen = _connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                _connection.Open();
            }

            try
            {
                _tableSelectionSet = tableSelectionSet;

                _databaseModel.DatabaseName = _connection.Database;
                _databaseModel.DefaultSchema = null;

                GetTables();
                GetColumns();
                GetIndexes();
                GetConstraints();
                return _databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    _connection.Close();
                }
            }
        }

        const string GetTablesQuery = @"select rdb$relation_name from rdb$relations where rdb$view_blr is null and (rdb$system_flag is null or rdb$system_flag = 0)";

        void GetTables()
        {
            using (var command = new FbCommand(GetTablesQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new DatabaseTable
                    {
                        Schema = null,
                        Name = reader.GetString(0)
                    };

                    if (_tableSelectionSet.Allows(table.Schema, table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                }
            }
        }

        const string GetColumnsQuery = @"SELECT
  RF.RDB$FIELD_NAME as Field,
  CASE F.RDB$FIELD_TYPE
    WHEN 7 THEN
      CASE F.RDB$FIELD_SUB_TYPE
        WHEN 0 THEN 'SMALLINT'
        WHEN 1 THEN 'NUMERIC(' || F.RDB$FIELD_PRECISION || ', ' || (-F.RDB$FIELD_SCALE) || ')'
        WHEN 2 THEN 'DECIMAL'
      END
    WHEN 8 THEN
      CASE F.RDB$FIELD_SUB_TYPE
        WHEN 0 THEN 'INTEGER'
        WHEN 1 THEN 'NUMERIC('  || F.RDB$FIELD_PRECISION || ', ' || (-F.RDB$FIELD_SCALE) || ')'
        WHEN 2 THEN 'DECIMAL'
      END
    WHEN 9 THEN 'QUAD'
    WHEN 10 THEN 'FLOAT'
    WHEN 12 THEN 'DATE'
    WHEN 13 THEN 'TIME'
    WHEN 14 THEN 'CHAR(' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ') '
    WHEN 16 THEN
      CASE F.RDB$FIELD_SUB_TYPE
        WHEN 0 THEN 'BIGINT'
        WHEN 1 THEN 'NUMERIC(' || F.RDB$FIELD_PRECISION || ', ' || (-F.RDB$FIELD_SCALE) || ')'
        WHEN 2 THEN 'DECIMAL'
      END
    WHEN 27 THEN 'DOUBLE'
    WHEN 35 THEN 'TIMESTAMP'
    WHEN 37 THEN 'VARCHAR(' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ')'
    WHEN 40 THEN 'CSTRING' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ')'
    WHEN 45 THEN 'BLOB_ID'
    WHEN 261 THEN 'BLOB SUB_TYPE ' || F.RDB$FIELD_SUB_TYPE
    ELSE 'RDB$FIELD_TYPE: ' || F.RDB$FIELD_TYPE || '?'
  END as Type,
  IIF(COALESCE(RF.RDB$NULL_FLAG, 0) = 0, 'YES', 'NO') as " + "\"" + @"Null"+ "\"" + @",
        COALESCE(RF.RDB$DEFAULT_SOURCE, F.RDB$DEFAULT_SOURCE) as " + "\"" + @"Default" + "\"" + @"
        FROM
            RDB$RELATION_FIELDS RF
        JOIN
            RDB$FIELDS F ON(F.RDB$FIELD_NAME = RF.RDB$FIELD_SOURCE)
        LEFT OUTER JOIN
            RDB$CHARACTER_SETS CH ON(CH.RDB$CHARACTER_SET_ID = F.RDB$CHARACTER_SET_ID)
        WHERE
            (RF.RDB$RELATION_NAME = {0}) AND(COALESCE(RF.RDB$SYSTEM_FLAG, 0) = 0)
        ORDER BY
        RF.RDB$FIELD_POSITION;";

        void GetColumns()
        {
            foreach (var x in _tables)
            {
                using (var command = new FbCommand(string.Format(GetColumnsQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        var column = new DatabaseColumn
                        {
                            Table = x.Value,
                            Name = reader.GetString(0),
                            StoreType = reader.GetString(1),
                            IsNullable = reader.GetString(2) == "YES",
                            DefaultValueSql = reader[3].ToString() == "" ? null : reader[3].ToString(),
                        };
                        x.Value.Columns.Add(column);
                    }
            }
        }

        const string GetPrimaryQuery = @"
select
    I.rdb$index_name as Index_Name,
    I.rdb$unique_flag as Non_Unique,
    I.rdb$relation_name as Columns
from
    RDB$INDICES I
where
   I.rdb$relation_name = {0}
   and I.rdb$index_name like 'PK_%'
group by
   Index_Name, Non_Unique, Columns";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        void GetPrimaryKeys()
        {
            foreach (var x in _tables)
            {
                using (var command = new FbCommand(string.Format(GetPrimaryQuery, x.Key.Replace("`", "")), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        try
                        {
                            var index = new DatabasePrimaryKey
                            {
                                Table = x.Value,
                                Name = reader.GetString(0),
                            };
                            
                            foreach (var column in reader.GetString(2).Split(','))
                            {
                                index.Columns.Add(x.Value.Columns.Single(y => y.Name == column));
                            }
                            
                            x.Value.PrimaryKey = index;
                        }
                        catch { }
                    }
            }
        }

        const string GetIndexesQuery = @"
select
    I.rdb$index_name as Index_Name,
    I.rdb$unique_flag as Non_Unique,
    I.rdb$relation_name as Columns
from
    RDB$INDICES I
where
   I.rdb$relation_name = {0}
   and I.rdb$index_name not like 'PK_%'
   and I.rdb$index_name not like 'FK_%'
group by
   Index_Name, Non_Unique, Columns";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        void GetIndexes()
        {
            foreach (var x in _tables)
            {
                using (var command = new FbCommand(string.Format(GetIndexesQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        try
                        {
                            var index = new DatabaseIndex
                            {
                                Table = x.Value,
                                Name = reader.GetString(0),
                                IsUnique = !reader.GetBoolean(1),
                            };

                            foreach (var column in reader.GetString(2).Split(','))
                            {
                                index.Columns.Add(x.Value.Columns.Single(y => y.Name == column));
                            }

                            x.Value.Indexes.Add(index);
                        }
                        catch { }
                    }
            }
        }

        const string GetConstraintsQuery = @"SELECT
    master_index_segments.rdb$index_name as CONSTRAINT_NAME,
    detail_relation_constraints.RDB$RELATION_NAME as SRC_TABLE_NAME,
    detail_index_segments.rdb$field_name AS SRC_COLUMN_NAME,
    master_relation_constraints.rdb$relation_name AS REFERENCED_TABLE_NAME,
    master_index_segments.rdb$field_name AS fk_field,
    rdb$ref_constraints.RDB$DELETE_RULE as Delete_Rule
FROM
    rdb$relation_constraints detail_relation_constraints
    JOIN rdb$index_segments detail_index_segments ON detail_relation_constraints.rdb$index_name = detail_index_segments.rdb$index_name 
    JOIN rdb$ref_constraints ON detail_relation_constraints.rdb$constraint_name = rdb$ref_constraints.rdb$constraint_name -- Master indeksas
    JOIN rdb$relation_constraints master_relation_constraints ON rdb$ref_constraints.rdb$const_name_uq = master_relation_constraints.rdb$constraint_name
    JOIN rdb$index_segments master_index_segments ON master_relation_constraints.rdb$index_name = master_index_segments.rdb$index_name
WHERE
    detail_relation_constraints.rdb$constraint_type = 'FOREIGN KEY'
    AND detail_relation_constraints.RDB$RELATION_NAME = {0}";

        void GetConstraints()
        {
            foreach (var x in _tables)
            {
                using (var command = new FbCommand(string.Format(GetConstraintsQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        if (_tables.ContainsKey($"{ reader.GetString(3) }"))
                        {
                            var fkInfo = new DatabaseForeignKey
                            {
                                Name = reader.GetString(0),
                                OnDelete = ConvertToReferentialAction(reader.GetString(5)),
                                Table = x.Value,
                                PrincipalTable = _tables[$"{ reader.GetString(3) }"]
                            };
                            fkInfo.Columns.Add(x.Value.Columns.Single(y => y.Name == reader.GetString(2)));
                            x.Value.ForeignKeys.Add(fkInfo);
                        }
                        else
                        {
                            Logger.LogWarning($"Referenced table { reader.GetString(4) } is not in dictionary.");
                        }
                    }
            }
        }
        private static ReferentialAction? ConvertToReferentialAction(string onDeleteAction)
        {
            switch (onDeleteAction.ToUpperInvariant())
            {
                case "SET DEFAUT":
                    return ReferentialAction.Restrict; // TODO

                case "CASCADE":
                    return ReferentialAction.Cascade;

                case "SET NULL":
                    return ReferentialAction.SetNull;

                case "NO ACTION":
                    return ReferentialAction.NoAction;

                default:
                    return null;
            }
        }
    }
}
