// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class FirebirdEntityTypeBuilderAnnotations : FirebirdEntityTypeAnnotations
    {
        public FirebirdEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder, ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToSchema([CanBeNull] string name)
            => SetSchema(Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToTable([CanBeNull] string name)
            => SetTableName(Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToTable([CanBeNull] string name, [CanBeNull] string schema)
        {
            var originalTable = TableName;
            if (!SetTableName(Check.NullButNotEmpty(name, nameof(name))))
            {
                return false;
            }

            if (!SetSchema(Check.NullButNotEmpty(schema, nameof(schema))))
            {
                SetTableName(originalTable);
                return false;
            }

            return true;
        }

#pragma warning disable 109
#pragma warning restore 109
    }
}
