// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
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
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"SHOW CREATE TABLE {sqlGenerationHelper.DelimitIdentifier(table, schema)}";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.GetFieldValue<string>(1);
                }
            }
            return null;
        }

    }
}
