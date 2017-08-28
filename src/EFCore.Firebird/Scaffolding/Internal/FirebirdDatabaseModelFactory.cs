using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

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
            Logger.LogDebug("ResetState");
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

                Logger.LogDebug("GetTables");
                GetTables();
                Logger.LogDebug("GetColumns");
                GetColumns();
                Logger.LogDebug("GetPrimaryKeys");
                GetPrimaryKeys();
                Logger.LogDebug("GetIndexes");
                GetIndexes();
                Logger.LogDebug("GetConstraints");
                GetConstraints();
                Logger.LogDebug("GetConstraints completed");

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

        const string GetTablesQuery = @"
select 
    rdb$relation_name 
from 
    rdb$relations 
where 
    rdb$view_blr is null 
    and (rdb$system_flag is null or rdb$system_flag = 0) 
    and rdb$relation_name not like 'IBE$%'";

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
                        Name = reader.GetString(0).Trim()
                    };

                    Logger.LogDebug($"GetTables => Add { reader.GetString(0).Trim() }.");

                    if (_tableSelectionSet.Allows(table.Schema, table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                }
            }
        }

        // DECIMAL(' || F.RDB$FIELD_PRECISION || ', ' || (-F.RDB$FIELD_SCALE) || ')

        readonly string _getColumnsQuery = $@"
SELECT
    RF.RDB$FIELD_NAME as Field,
    CASE Coalesce(F.RDB$FIELD_TYPE, 0)
    WHEN 7 THEN
        CASE F.RDB$FIELD_SUB_TYPE
        WHEN 0 THEN 'SMALLINT'
        ELSE 'DECIMAL'
        END
    WHEN 8 THEN
        CASE F.RDB$FIELD_SUB_TYPE
        WHEN 0 THEN 'INTEGER'
        ELSE 'DECIMAL'
        END
    WHEN 9 THEN 'QUAD'
    WHEN 10 THEN 'FLOAT'
    WHEN 12 THEN 'DATE'
    WHEN 13 THEN 'TIME'
    WHEN 14 THEN 'CHAR(' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ') '
    WHEN 16 THEN
        CASE F.RDB$FIELD_SUB_TYPE
        WHEN 0 THEN 'BIGINT'
        ELSE 'DECIMAL'
        END
    WHEN 27 THEN 'DOUBLE PRECISION'
    WHEN 35 THEN 'TIMESTAMP'
    WHEN 37 THEN 'VARCHAR(' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ')'
    WHEN 40 THEN 'CSTRING' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ')'
    WHEN 45 THEN 'BLOB_ID'
    WHEN 261 THEN 'BLOB SUB_TYPE ' || F.RDB$FIELD_SUB_TYPE
    ELSE 'RDB$FIELD_TYPE: ' || F.RDB$FIELD_TYPE || '?'
    END as Type,
    IIF(COALESCE(RF.RDB$NULL_FLAG, 0) = 0, 'YES', 'NO') as sNull,
    COALESCE(RF.RDB$DEFAULT_SOURCE, F.RDB$DEFAULT_SOURCE) as AsDefault,
    RF.rdb$description
FROM
    RDB$RELATION_FIELDS RF
JOIN
    RDB$FIELDS F ON(F.RDB$FIELD_NAME = RF.RDB$FIELD_SOURCE)
LEFT OUTER JOIN
    RDB$CHARACTER_SETS CH ON(CH.RDB$CHARACTER_SET_ID = F.RDB$CHARACTER_SET_ID)
WHERE
    (RF.RDB$RELATION_NAME = '" +"{0}"+@"') AND(COALESCE(RF.RDB$SYSTEM_FLAG, 0) = 0)
ORDER BY
RF.RDB$FIELD_POSITION;";

        void GetColumns()
        {
            foreach (var x in _tables)
            {
                using (var command = new FbCommand(string.Format(_getColumnsQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        var column = new DatabaseColumn
                        {
                            Table = x.Value,
                            Name = reader.GetString(0).Trim(),
                            StoreType = reader.GetString(1).Trim(),
                            IsNullable = reader.GetString(2).Trim() == "YES",
                            DefaultValueSql = reader[3].ToString() == "" ? null : reader[3].ToString(),
                        };

                        if (!string.IsNullOrEmpty(reader.GetString(4)))
                            column.AddAnnotation("Description", reader.GetString(4).Trim().Replace(Environment.NewLine, "; "));

                        x.Value.Columns.Add(column);
                    }
                Logger.LogDebug($"GetColumns => {x.Value.Columns.Count} columns for table {x.Value.Name}");
            }
        }

        const string GetPrimaryQuery = @"SELECT
   i.rdb$index_name as index_name,
   sg.rdb$field_name as field_name
FROM
    RDB$INDICES i
    LEFT JOIN rdb$index_segments sg on i.rdb$index_name = sg.rdb$index_name
    LEFT JOIN rdb$relation_constraints rc on rc.rdb$index_name = I.rdb$index_name
WHERE
    rc.rdb$constraint_type = 'PRIMARY KEY'
AND
    i.rdb$relation_name = '{0}'";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        void GetPrimaryKeys()
        {
            foreach (var x in _tables)
            {
                DatabasePrimaryKey index = null;

                using (var command = new FbCommand(string.Format(GetPrimaryQuery, x.Key.Replace("\"", "")), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        //try
                        //{
                        if (index == null)
                            index = new DatabasePrimaryKey
                            {
                                Table = x.Value,
                                Name = reader.GetString(0).Trim()
                            };

                        index.Columns.Add(x.Value.Columns.Single(y => y.Name == reader.GetString(1).Trim()));
                        //}
                        //catch { }
                    }

                x.Value.PrimaryKey = index;

                if(x.Value.PrimaryKey != null)
                    Logger.LogDebug($"GetPrimaryKeys => pk for table {x.Key} found => {x.Value.PrimaryKey.Name} with columns {string.Join(",", x.Value.PrimaryKey.Columns.Select(c=>c.Name))}");
                else
                    Logger.LogDebug($"GetPrimaryKeys => pk for table {x.Key} not found");
            }
        }

        const string GetIndexesQuery = @"
SELECT
    I.rdb$index_name as Index_Name,
    COALESCE(I.rdb$unique_flag, 0) as Non_Unique,
    I.rdb$relation_name as Columns
FROM
    RDB$INDICES i
    LEFT JOIN rdb$index_segments sg on i.rdb$index_name = sg.rdb$index_name
    LEFT JOIN rdb$relation_constraints rc on rc.rdb$index_name = I.rdb$index_name and rc.rdb$constraint_type = null
WHERE
   i.rdb$relation_name = '{0}'
GROUP BY
   Index_Name, Non_Unique, Columns";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        void GetIndexes()
        {
            foreach (var x in _tables)
            {
                DatabaseIndex index = null;
                using (var command = new FbCommand(string.Format(GetIndexesQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        try
                        {
                            if(index == null)
                                index = new DatabaseIndex
                                {
                                    Table = x.Value,
                                    Name = reader.GetString(0).Trim(),
                                    IsUnique = !reader.GetBoolean(1),
                                };

                            foreach (var column in reader.GetString(2).Trim().Split(','))
                            {
                                index.Columns.Add(x.Value.Columns.Single(y => y.Name == column));
                            }

                            x.Value.Indexes.Add(index);
                        }
                        catch { }
                    }

                Logger.LogDebug($"GetIndexes => Table {x.Key} => " + (x.Value.Indexes != null ? $"{x.Value.Indexes.Count}" : "0") + " index found");
            }
        }

        const string GetConstraintsQuery = @"
SELECT
    drs.rdb$constraint_name as PK_NAME,
    LIST(distinct trim(dis.rdb$field_name)||'#'||dis.rdb$field_position,',') AS SRC_COLUMN_NAME,
    mrc.rdb$relation_name AS PRINC_TABLE_NAME,
    LIST(distinct trim(mis.rdb$field_name)||'#'||mis.rdb$field_position,',') AS PRINC_COLUMN_NAME,
    rc.RDB$DELETE_RULE as DELETE_RULE
FROM
    rdb$relation_constraints drs
    left JOIN rdb$index_segments dis ON drs.rdb$index_name = dis.rdb$index_name
    left JOIN rdb$ref_constraints rc ON drs.rdb$constraint_name = rc.rdb$constraint_name
    left JOIN rdb$relation_constraints mrc ON rc.rdb$const_name_uq = mrc.rdb$constraint_name
    left JOIN rdb$index_segments mis ON mrc.rdb$index_name = mis.rdb$index_name
WHERE
    drs.rdb$constraint_type = 'FOREIGN KEY'
    AND drs.RDB$RELATION_NAME = '{0}'
GROUP BY
   drs.rdb$constraint_name,
   mrc.rdb$relation_name,
   rc.RDB$DELETE_RULE";

        void GetConstraints()
        {
            foreach (var x in _tables)
            {
                using (var command = new FbCommand(string.Format(GetConstraintsQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        if (_tables.ContainsKey($"{x.Key}"))
                        {
                            DatabaseForeignKey fkInfo = new DatabaseForeignKey
                            {
                                Table = x.Value,
                                Name = reader.GetString(0).Trim(),
                                OnDelete = ConvertToReferentialAction(reader.GetString(4)),
                                PrincipalTable = _tables[reader.GetString(2).Trim()]
                            };

                            // TODO: the following code is ugly, must refactor (o_0)

                            Logger.LogDebug(
                                $"   PK ==> Table {x.Value.Name} => {reader.GetString(0).Trim()}, PrincipalTable {fkInfo.PrincipalTable.Name}, FK {reader.GetString(0).Trim()} => {reader.GetString(1).Trim()}, {reader.GetString(3).Trim()}");
                            
                            var fkcols = reader.GetString(1).Split(',');
                            var columns = new string[fkcols.Length];

                            foreach (var colandpos in fkcols)
                            {
                                var split = colandpos.Split('#');
                                columns[int.Parse(split[1])] = split[0];
                            }

                            foreach (var column in columns)
                                fkInfo.Columns.Add(x.Value.Columns.Single(y => y.Name == column));

                            var fkcols2 = reader.GetString(1).Split(',');
                            var columns2 = new string[fkcols2.Length];

                            foreach (var colandpos in fkcols2)
                            {
                                var split = colandpos.Split('#');
                                columns2[int.Parse(split[1])] = split[0];
                            }

                            /*columns.Clear();
                            foreach (var colandpos in reader.GetString(3).Split(','))
                            {
                                var split = colandpos.Split('#');
                                columns.Insert(int.Parse(split[1]), split[0]);
                            }*/

                            foreach (var column in columns2)
                                fkInfo.PrincipalColumns.Add(x.Value.Columns.Single(y => y.Name == column));

                            //foreach (var column in reader.GetString(3).Split(','))
                            //    fkInfo.PrincipalColumns.Add(x.Value.Columns.Single(y => y.Name == column));
                            
                            x.Value.ForeignKeys.Add(fkInfo);
                        }
                        else
                        {
                            Logger.LogWarning($"GetConstraints => Referenced table { reader.GetString(3).Trim() } is not in dictionary.");
                        }
                    }
                Logger.LogDebug($"GetConstraints => Table {x.Key} => {x.Value.ForeignKeys.Count}");
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
