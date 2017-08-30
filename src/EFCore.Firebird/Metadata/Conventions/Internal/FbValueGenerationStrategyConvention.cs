// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FbValueGenerationStrategyConvention : DatabaseGeneratedAttributeConvention, IModelInitializedConvention
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, DatabaseGeneratedAttribute attribute, MemberInfo clrMember)
        {
            FirebirdValueGenerationStrategy? valueGenerationStrategy = null;
            ValueGenerated valueGenerated = ValueGenerated.Never;
            if (attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed)
            {
                valueGenerated = ValueGenerated.OnAddOrUpdate;
                valueGenerationStrategy = FirebirdValueGenerationStrategy.ComputedColumn;
            }
            else if (attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
            {
                valueGenerated = ValueGenerated.OnAdd;
                valueGenerationStrategy = FirebirdValueGenerationStrategy.IdentityColumn;
            }

            propertyBuilder.ValueGenerated(valueGenerated, ConfigurationSource.Convention);
            propertyBuilder.Firebird(ConfigurationSource.DataAnnotation).ValueGenerationStrategy(valueGenerationStrategy);

            return base.Apply(propertyBuilder, attribute, clrMember);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.Firebird(ConfigurationSource.Convention).ValueGenerationStrategy(FirebirdValueGenerationStrategy.IdentityColumn);

            return modelBuilder;
        }
    }
}
