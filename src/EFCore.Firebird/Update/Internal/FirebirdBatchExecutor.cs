// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{

    public class FirebirdBatchExecutor : IBatchExecutor
    {

        public int Execute(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection)
        {
            var rowsAffected = 0;
            connection.Open();
            IDbContextTransaction startedTransaction = null;
            try
            {
                if (connection.CurrentTransaction == null)
                {
                	startedTransaction = connection.BeginTransaction();
                }

                foreach (var commandbatch in commandBatches)
                {
                    commandbatch.Execute(connection);
                    rowsAffected += commandbatch.ModificationCommands.Count;
                }

                startedTransaction?.Commit();
                startedTransaction?.Dispose();
            }
            catch
            {
                try
                {
                    startedTransaction?.Rollback();
                    startedTransaction?.Dispose();
                }
                catch
                {
                    // if the connection was lost, rollback command will fail.  prefer to throw original exception in that case
                }
                throw;
            }
            finally
            {
                connection.Close();
            }

            return rowsAffected;
        }

        public async Task<int> ExecuteAsync(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var rowsAffected = 0;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            FirebirdRelationalTransaction startedTransaction = null;
            try
            {
                if (connection.CurrentTransaction == null)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    startedTransaction = await (connection as FirebirdRelationalConnection).BeginTransactionAsync(cancellationToken).ConfigureAwait(false) as FirebirdRelationalTransaction;
                }

                foreach (var commandbatch in commandBatches)
                {
                    await commandbatch.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                    rowsAffected += commandbatch.ModificationCommands.Count;
                }

                if (startedTransaction != null)
                {
                  await startedTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                startedTransaction?.Dispose();
            }
            catch(Exception err)
            {
                try
                {
                    startedTransaction?.Rollback();
                    startedTransaction?.Dispose();
                }
                catch
                {
                    // if the connection was lost, rollback command will fail.  prefer to throw original exception in that case
                }
                throw err;
            }
            finally
            {
                connection.Close();
            }

            return rowsAffected;
        }
    }
}
