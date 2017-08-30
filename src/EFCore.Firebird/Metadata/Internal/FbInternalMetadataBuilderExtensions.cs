// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class FbInternalMetadataBuilderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FbModelBuilderAnnotations Firebird(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new FbModelBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FbPropertyBuilderAnnotations Firebird(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new FbPropertyBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FbEntityTypeBuilderAnnotations Firebird(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new FbEntityTypeBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FbKeyBuilderAnnotations Firebird(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new FbKeyBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FbIndexBuilderAnnotations Firebird(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new FbIndexBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static RelationalForeignKeyBuilderAnnotations Firebird(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource);
    }
}
