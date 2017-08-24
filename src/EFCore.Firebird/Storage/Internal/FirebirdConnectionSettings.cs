// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FbConnectionSettings
    {
        private static readonly ConcurrentDictionary<string, FbConnectionSettings> Settings
            = new ConcurrentDictionary<string, FbConnectionSettings>();

        private static FbConnectionStringBuilder _settingsCsb(FbConnectionStringBuilder csb)
        {
            return new FbConnectionStringBuilder
            {                
                Database = csb.Database,
                Port = csb.Port,
                DataSource = csb.DataSource
            };
        }

        public static FbConnectionSettings GetSettings(string connectionString)
        {
            var csb = new FbConnectionStringBuilder(connectionString);
            var settingsCsb = _settingsCsb(csb);
            return Settings.GetOrAdd(settingsCsb.ConnectionString, key =>
            {
                csb.Pooling = false;
                string serverVersion;
                using (var schemalessConnection = new FbConnection(csb.ConnectionString))
                {
                    schemalessConnection.Open();
                    serverVersion = schemalessConnection.ServerVersion;
                }
                var version = new ServerVersion(serverVersion);
                return new FbConnectionSettings(settingsCsb, version);
            });
        }

        public static FbConnectionSettings GetSettings(DbConnection connection)
        {
            var csb = new FbConnectionStringBuilder(connection.ConnectionString);
            var settingsCsb = _settingsCsb(csb);
            return Settings.GetOrAdd(settingsCsb.ConnectionString, key =>
            {
                var opened = false;
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                    opened = true;
                }
                try
                {
                    var version = new ServerVersion(connection.ServerVersion);
                    var connectionSettings = new FbConnectionSettings(settingsCsb, version);
                    return connectionSettings;
                }
                finally
                {
                    if (opened)
                        connection.Close();
                }
            });
        }

        private FbConnectionSettings(FbConnectionStringBuilder settingsCsb, ServerVersion serverVersion)
        {
            // Settings from the connection string
            ServerVersion = serverVersion;
        }

        public readonly ServerVersion ServerVersion;

    }
}
