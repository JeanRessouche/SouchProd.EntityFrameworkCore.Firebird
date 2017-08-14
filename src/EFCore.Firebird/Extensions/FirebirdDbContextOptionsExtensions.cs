// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using FirebirdSql.Data.FirebirdClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class FirebirdDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder UseFirebird(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<FbDbContextOptionsBuilder> FirebirdOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));


            var csb = new FbConnectionStringBuilder(connectionString)
            {
                
            };
            
            connectionString = csb.ConnectionString;

            var extension = GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            
            FirebirdOptionsAction?.Invoke(new FbDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder UseFirebird(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<FbDbContextOptionsBuilder> FirebirdOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var csb = new FbConnectionStringBuilder(connection.ConnectionString);

            /*if (csb.AllowUserVariables != true || csb.BufferResultSets != true || csb.UseAffectedRows != false)
	        {
	            try
	            {
		            csb.AllowUserVariables = true;
                    csb.BufferResultSets = true;
		            csb.UseAffectedRows = false;
		            connection.ConnectionString = csb.ConnectionString;
	            }
	            catch (FirebirdException e)
                {
                    throw new InvalidOperationException("The Firebird Connection string used with Pomelo.EntityFrameworkCore.Firebird " +
                    	"must contain \"AllowUserVariables=true;BufferResultSets=true;UseAffectedRows=false\"", e);
                }
            }*/
            
            var extension = GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            FirebirdOptionsAction?.Invoke(new FbDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseFirebird<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<FbDbContextOptionsBuilder> FirebirdOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseFirebird(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, FirebirdOptionsAction);

        public static DbContextOptionsBuilder<TContext> UseFirebird<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<FbDbContextOptionsBuilder> FirebirdOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseFirebird(
                (DbContextOptionsBuilder)optionsBuilder, connection, FirebirdOptionsAction);

        private static FbOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<FbOptionsExtension>()
               ?? new FbOptionsExtension();

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                  ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
