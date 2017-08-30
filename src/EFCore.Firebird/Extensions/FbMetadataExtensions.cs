// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for metadata.
    /// </summary>
    public static class FbMetadataExtensions
    {
        /// <summary>
        ///     Gets the SQL Server specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the property. </returns>
        public static FbPropertyAnnotations Firebird([NotNull] this IMutableProperty property)
            => (FbPropertyAnnotations)Firebird((IProperty)property);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the property. </returns>
        public static IFirebirdPropertyAnnotations Firebird([NotNull] this IProperty property)
            => new FbPropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the entity. </returns>
        public static FbEntityTypeAnnotations Firebird([NotNull] this IMutableEntityType entityType)
            => (FbEntityTypeAnnotations)Firebird((IEntityType)entityType);

        /// <summary>
        ///     Gets the SQL Server specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the entity. </returns>
        public static IFirebirdEntityTypeAnnotations Firebird([NotNull] this IEntityType entityType)
            => new FbEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the key. </returns>
        public static FbKeyAnnotations Firebird([NotNull] this IMutableKey key)
            => (FbKeyAnnotations)Firebird((IKey)key);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the key. </returns>
        public static IFirebirdKeyAnnotations Firebird([NotNull] this IKey key)
            => new FbKeyAnnotations(Check.NotNull(key, nameof(key)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the index. </returns>
        public static FbIndexAnnotations Firebird([NotNull] this IMutableIndex index)
            => (FbIndexAnnotations)Firebird((IIndex)index);

        /// <summary>
        ///     Gets the SQL Server specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the index. </returns>
        public static IFirebirdIndexAnnotations Firebird([NotNull] this IIndex index)
            => new FbIndexAnnotations(Check.NotNull(index, nameof(index)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the model. </returns>
        public static FbModelAnnotations Firebird([NotNull] this IMutableModel model)
            => (FbModelAnnotations)Firebird((IModel)model);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the model. </returns>
        public static IFirebirdModelAnnotations Firebird([NotNull] this IModel model)
            => new FbModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
