// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class FirebirdServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkFirebird([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<FbOptionsExtension>>()
                .TryAdd<IRelationalTypeMapper, FirebirdSmartTypeMapper>()
                .TryAdd<ISqlGenerationHelper, FirebirdSqlGenerationHelper>()
                .TryAdd<IMigrationsAnnotationProvider, FirebirdMigrationsAnnotationProvider>()
                .TryAdd<IConventionSetBuilder, FirebirdConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator>(p => p.GetService<IFirebirdUpdateSqlGenerator>())
                .TryAdd<IModificationCommandBatchFactory, FirebirdModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, FirebirdValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetService<IFirebirdRelationalConnection>())
                .TryAdd<IRelationalCommandBuilderFactory, FbCommandBuilderFactory>()
                .TryAdd<IMigrationsSqlGenerator, FirebirdMigrationsSqlGenerator>()
                .TryAdd<IBatchExecutor, FirebirdBatchExecutor>()
                .TryAdd<IRelationalDatabaseCreator, FirebirdDatabaseCreator>()
                .TryAdd<IHistoryRepository, FirebirdHistoryRepository>()
                .TryAdd<IMemberTranslator, FirebirdCompositeMemberTranslator>()
                .TryAdd<ICompositeMethodCallTranslator, FirebirdCompositeMethodCallTranslator>()
                .TryAdd<IQuerySqlGeneratorFactory, FirebirdQuerySqlGeneratorFactory>()
                .TryAdd<ISingletonOptions, IFirebirdOptions>(p => p.GetService<IFirebirdOptions>())
                .TryAddProviderSpecificServices(b => b
                    .TryAddSingleton<IFirebirdOptions, FirebirdOptions>()
                    .TryAddScoped<IFirebirdUpdateSqlGenerator, FirebirdUpdateSqlGenerator>()
                    .TryAddScoped<IFirebirdRelationalConnection, FirebirdRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
