// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class FbConventionSetBuilder : RelationalConventionSetBuilder
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        private readonly IFirebirdOptions _options;

        public FbConventionSetBuilder(
            [NotNull] RelationalConventionSetBuilderDependencies dependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IFirebirdOptions options)
            : base(dependencies)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
            _options = options;
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new FbValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            
            ReplaceConvention(conventionSet.PropertyAddedConventions, (DatabaseGeneratedAttributeConvention)valueGenerationStrategyConvention);
            ReplaceConvention(conventionSet.PropertyFieldChangedConventions, (DatabaseGeneratedAttributeConvention)valueGenerationStrategyConvention);

            return conventionSet;
        }

        /*public static ConventionSet Build()
        {
            var sqlServerTypeMapper = new FbTypeMapper(new RelationalTypeMapperDependencies());

            return new FirebirdConventionSetBuilder(
                    new RelationalConventionSetBuilderDependencies(sqlServerTypeMapper, null, null),
                    new FbSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
                .AddConventions(
                    new CoreConventionSetBuilder(
                            new CoreConventionSetBuilderDependencies(sqlServerTypeMapper))
                        .CreateConventionSet());
        }*/
    }
}
