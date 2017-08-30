using System.Data.Common;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using JetBrains.Annotations;
using System.Data;
using System.Threading;
using System;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FbRelationalConnection : RelationalConnection, IFirebirdRelationalConnection
    {
        
        public FbRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override DbConnection CreateDbConnection() => new FbConnection(ConnectionString);

        public virtual IFirebirdRelationalConnection CreateMasterConnection()
        {
            var csb = new FbConnectionStringBuilder(ConnectionString)
            {
                Database = "",
                Pooling = false
            };

            var contextOptions = new DbContextOptionsBuilder()
                .UseFirebird(csb.ConnectionString)
                .Options;
                
            return new FbRelationalConnection(Dependencies.With(contextOptions));
        }

        public override bool IsMultipleActiveResultSetsEnabled => true;

        [NotNull]
        public override async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
            }

            await OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return await BeginTransactionWithNoPreconditionsAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IDbContextTransaction> BeginTransactionWithNoPreconditionsAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken=default(CancellationToken))
        {
            var dbTransaction = (DbConnection as FbConnection).BeginTransaction(isolationLevel);

            CurrentTransaction
                = new FbRelationalTransaction(
                    this,
                    dbTransaction,
                    Dependencies.TransactionLogger,
                    transactionOwned: true);
            
            Dependencies.TransactionLogger.TransactionStarted(
                this, 
                dbTransaction, 
                CurrentTransaction.TransactionId,
                DateTimeOffset.UtcNow);

            return CurrentTransaction;
        }

        /// <summary>
        ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        public override IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            if (transaction == null)
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction = null;
                }
            }
            else
            {
                if (CurrentTransaction != null)
                {
                    throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
                }

                Open();

                CurrentTransaction = new FbRelationalTransaction(
                    this, 
                    transaction, 
                    Dependencies.TransactionLogger, 
                    transactionOwned: false);

                Dependencies.TransactionLogger.TransactionUsed(
                    this, 
                    transaction, 
                    CurrentTransaction.TransactionId,
                    DateTimeOffset.UtcNow);
            }

            return CurrentTransaction;
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken=default(CancellationToken))
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            await (CurrentTransaction as FbRelationalTransaction).CommitAsync().ConfigureAwait(false);
        }

        public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken=default(CancellationToken))
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            await (CurrentTransaction as FbRelationalTransaction).RollbackAsync().ConfigureAwait(false);
        }
    }
}
