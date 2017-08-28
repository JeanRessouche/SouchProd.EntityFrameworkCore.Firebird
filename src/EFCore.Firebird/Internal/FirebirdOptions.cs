// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using FirebirdSql.Data.FirebirdClient;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class FirebirdOptions : IFirebirdOptions
    {

        private FbOptionsExtension _relationalOptions;

        private readonly Lazy<FbConnectionSettings> _lazyConnectionSettings;

        public FirebirdOptions()
        {
            _lazyConnectionSettings = new Lazy<FbConnectionSettings>(() =>
            {
                if (_relationalOptions.Connection != null)
                    return FbConnectionSettings.GetSettings(_relationalOptions.Connection);
                return FbConnectionSettings.GetSettings(_relationalOptions.ConnectionString);
            });
        }

        public virtual void Initialize(IDbContextOptions options)
        {
            _relationalOptions = options.FindExtension<FbOptionsExtension>() ?? new FbOptionsExtension();
        }

        public virtual void Validate(IDbContextOptions options)
        {
            if (_relationalOptions.ConnectionString == null && _relationalOptions.Connection == null)
                throw new InvalidOperationException(RelationalStrings.NoConnectionOrConnectionString);
        }

        public virtual FbConnectionSettings ConnectionSettings => _lazyConnectionSettings.Value;

        public virtual string GetCreateTable(ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            if (_relationalOptions.Connection != null)
                return GetCreateTable(_relationalOptions.Connection, sqlGenerationHelper, table, schema);
            return GetCreateTable(_relationalOptions.ConnectionString, sqlGenerationHelper, table, schema);
        }

        private static string GetCreateTable(string connectionString, ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();
                return ExecuteCreateTable(connection, sqlGenerationHelper, table, schema);
            }
        }

        private static string GetCreateTable(DbConnection connection, ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            var opened = false;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
                opened = true;
            }
            try
            {
                return ExecuteCreateTable(connection, sqlGenerationHelper, table, schema);
            }
            finally
            {
                if (opened)
                    connection.Close();
            }
        }

        private static string ExecuteCreateTable(DbConnection connection, ISqlGenerationHelper sqlGenerationHelper, string table, string schema)
        {
            Debug.WriteLine("ExecuteCreateTable");

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"select
                rf.rdb$field_name as column_name,
                case f.rdb$field_type
                    when 261 then 'BLOB SUB_TYPE ' || F.RDB$FIELD_SUB_TYPE
                    when 37 then 'VARCHAR(' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ')'
                when 35 then 'TIMESTAMP'
                when 27 then 'DOUBLE PRECISION'
                when 16 then
                case f.rdb$field_sub_type
                    when 0 then 'BIGINT'
                when 1 then 'NUMERIC'
                when 2 then 'DECIMAL'
                end
                    when 14 then 'CHAR(' || (TRUNC(F.RDB$FIELD_LENGTH / CH.RDB$BYTES_PER_CHARACTER)) || ') '
                when 13 then 'TIME'
                when 12 then 'DATE'
                when 10 then 'FLOAT'
                when 9 then 'QUAD'
                when 8 then
                case f.rdb$field_sub_type
                    when 0 then 'INTEGER'
                when 1 then 'NUMERIC'
                when 2 then 'DECIMAL'
                end
                    when 7 then
                case f.rdb$field_sub_type
                    when 0 then 'SMALLINT'
                when 1 then 'NUMERIC'
                when 2 then 'DECIMAL'
                end
                end as data_type,
                IIF(COALESCE(RF.RDB$NULL_FLAG, 0) = 0, null, 'NOT NULL') as nullable
                    from rdb$fields f
                join rdb$relation_fields rf on rf.rdb$field_source = f.rdb$field_name
                    left join rdb$character_sets ch on(CH.RDB$CHARACTER_SET_ID = F.RDB$CHARACTER_SET_ID)
                where rf.rdb$relation_name = '{sqlGenerationHelper.DelimitIdentifier(table, schema)}'";

                Debug.WriteLine("ExecuteCreateTable = > " + cmd.CommandText);

                var sb = new StringBuilder();
                sb.AppendLine($"CREATE TABLE {sqlGenerationHelper.DelimitIdentifier(table, schema)} (");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sb.AppendLine($"{reader.GetFieldValue<string>(0)}   {(reader.GetFieldValue<string>(1) + " " +reader.GetFieldValue<string>(2)).Trim()},");
                    }
                }

                sb.AppendLine(");");

                Debug.WriteLine("ExecuteCreateTable = > List PK");

                string GetPrimaryQuery = $@"
select
    I.rdb$index_name as Index_Name,
    I.rdb$unique_flag as Non_Unique,
    I.rdb$relation_name as Columns
from
    RDB$INDICES I
where
   I.rdb$relation_name = '{sqlGenerationHelper.DelimitIdentifier(table, schema)}'
   and I.rdb$index_name like 'PK_%'
group by
   Index_Name, Non_Unique, Columns";

                cmd.CommandText = GetPrimaryQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sb.AppendLine(
                            $"ALTER TABLE {sqlGenerationHelper.DelimitIdentifier(table, schema)} ADD CONSTRAINT {reader.GetFieldValue<string>(0)} PRIMARY KEY ({reader.GetFieldValue<string>(2)})");
                    }
                }

                Debug.WriteLine("ExecuteCreateTable = > List Idx");

                string GetIndexesQuery = $@"
select
    I.rdb$index_name as Index_Name,
    I.rdb$unique_flag as Non_Unique,
    I.rdb$relation_name as Columns
from
    RDB$INDICES I
where
   I.rdb$relation_name = '{sqlGenerationHelper.DelimitIdentifier(table, schema)}'
   and I.rdb$index_name not like 'PK_%'
   and I.rdb$index_name not like 'FK_%'
group by
   Index_Name, Non_Unique, Columns";

                cmd.CommandText = GetIndexesQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sb.AppendLine(
                            $"CREATE INDEX {reader.GetFieldValue<string>(0)} ON {sqlGenerationHelper.DelimitIdentifier(table, schema)} ({reader.GetFieldValue<string>(2)})");
                    }
                }

                Debug.WriteLine("ExecuteCreateTable = > " + sb.ToString());

                return sb.ToString();

                // TODO: Triggers
            }
            return null;
        }

    }
}
