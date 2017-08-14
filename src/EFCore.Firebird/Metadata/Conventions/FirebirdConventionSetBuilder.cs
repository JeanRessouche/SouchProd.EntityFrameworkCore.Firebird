// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class FirebirdConventionSetBuilder : RelationalConventionSetBuilder
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        public FirebirdConventionSetBuilder(
            [NotNull] RelationalConventionSetBuilderDependencies dependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(dependencies)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new FirebirdValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            
            ReplaceConvention(conventionSet.PropertyAddedConventions, (DatabaseGeneratedAttributeConvention)valueGenerationStrategyConvention);
            ReplaceConvention(conventionSet.PropertyFieldChangedConventions, (DatabaseGeneratedAttributeConvention)valueGenerationStrategyConvention);

            return conventionSet;
        }

        public static ConventionSet Build()
        {
            var sqlServerTypeMapper = new FirebirdTypeMapper(new RelationalTypeMapperDependencies());

            return new FirebirdConventionSetBuilder(
                    new RelationalConventionSetBuilderDependencies(sqlServerTypeMapper, null, null),
                    new FirebirdSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
                .AddConventions(
                    new CoreConventionSetBuilder(
                            new CoreConventionSetBuilderDependencies(sqlServerTypeMapper))
                        .CreateConventionSet());
        }
    }
}
