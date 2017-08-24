// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using FirebirdSql.Data.FirebirdClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FirebirdDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IFirebirdRelationalConnection _connection;
	    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;


	    /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
	    public FirebirdDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] IFirebirdRelationalConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

	    public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken).ConfigureAwait(false);

                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override bool HasTables()
            => Dependencies.ExecutionStrategyFactory.Create().Execute(_connection, connection => (long)CreateHasTablesCommand().ExecuteScalar(connection) != 0);

        protected override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(_connection,
                async (connection, ct) => (long)await CreateHasTablesCommand().ExecuteScalarAsync(connection, cancellationToken: ct).ConfigureAwait(false) != 0, cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build(@"
select
    CASE WHEN COUNT(*) = 0 THEN 0 ELSE 1 END
from
    rdb$relations
where
    rdb$view_blr is null and (rdb$system_flag is null or rdb$system_flag = 0)");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
        {
            var builder = new FbConnectionStringBuilder(_connection.DbConnection.ConnectionString);
            return Dependencies.MigrationsSqlGenerator.Generate((new[] { new FirebirdCreateDatabaseOperation { Name = _connection.DbConnection.Database } }));
        }
        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => Dependencies.ExecutionStrategyFactory.Create().Execute(DateTime.UtcNow + RetryTimeout, giveUp =>
                {
                    while (true)
                    {
                        try
                        {
                            _connection.DbConnection.Open();
                            _connection.DbConnection.Close();
                            return true;
                        }
                        catch (FbException e)
                        {
                            if (!retryOnNotExists && IsDoesNotExist(e))
                                return false;

                            if (DateTime.UtcNow > giveUp || !RetryOnExistsFailure(e))
                                throw;

                            Thread.Sleep(RetryDelay);
                        }
                    }
                });

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
                {
                    while (true)
                    {
                        try
                        {
                            await _connection.DbConnection.OpenAsync(ct).ConfigureAwait(false);
                            _connection.DbConnection.Close();
                            return true;
                        }
                        catch (FbException e)
                        {
                            if (!retryOnNotExists && IsDoesNotExist(e))
                                return false;

                            if (DateTime.UtcNow > giveUp || !RetryOnExistsFailure(e))
                                throw;

                            await Task.Delay(RetryDelay, ct).ConfigureAwait(false);
                        }
                    }
                }, cancellationToken);

        // Login failed is thrown when database does not exist (See Issue #776)
        private static bool IsDoesNotExist(FbException exception) => exception.ErrorCode == 1049;

        // See Issue #985
        private bool RetryOnExistsFailure(FbException exception)
        {
            if (exception.ErrorCode == 1049)
            {
                ClearPool();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken).ConfigureAwait(false);
            }
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new FirebirdDropDatabaseOperation { Name = _connection.DbConnection.Database }
            };

            var masterCommands = Dependencies.MigrationsSqlGenerator.Generate(operations);
            return masterCommands;
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools() => FbConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => FbConnection.ClearPool(_connection.DbConnection as FbConnection);
    }
}
