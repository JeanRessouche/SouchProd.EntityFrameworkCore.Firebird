// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
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
    public static class FirebirdMetadataExtensions
    {
        /// <summary>
        ///     Gets the SQL Server specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the property. </returns>
        public static FirebirdPropertyAnnotations Firebird([NotNull] this IMutableProperty property)
            => (FirebirdPropertyAnnotations)Firebird((IProperty)property);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the property. </returns>
        public static IFirebirdPropertyAnnotations Firebird([NotNull] this IProperty property)
            => new FirebirdPropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the entity. </returns>
        public static FirebirdEntityTypeAnnotations Firebird([NotNull] this IMutableEntityType entityType)
            => (FirebirdEntityTypeAnnotations)Firebird((IEntityType)entityType);

        /// <summary>
        ///     Gets the SQL Server specific metadata for an entity.
        /// </summary>
        /// <param name="entityType"> The entity to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the entity. </returns>
        public static IFirebirdEntityTypeAnnotations Firebird([NotNull] this IEntityType entityType)
            => new FirebirdEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the key. </returns>
        public static FirebirdKeyAnnotations Firebird([NotNull] this IMutableKey key)
            => (FirebirdKeyAnnotations)Firebird((IKey)key);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a key.
        /// </summary>
        /// <param name="key"> The key to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the key. </returns>
        public static IFirebirdKeyAnnotations Firebird([NotNull] this IKey key)
            => new FirebirdKeyAnnotations(Check.NotNull(key, nameof(key)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the index. </returns>
        public static FirebirdIndexAnnotations Firebird([NotNull] this IMutableIndex index)
            => (FirebirdIndexAnnotations)Firebird((IIndex)index);

        /// <summary>
        ///     Gets the SQL Server specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the index. </returns>
        public static IFirebirdIndexAnnotations Firebird([NotNull] this IIndex index)
            => new FirebirdIndexAnnotations(Check.NotNull(index, nameof(index)));

        /// <summary>
        ///     Gets the SQL Server specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the model. </returns>
        public static FirebirdModelAnnotations Firebird([NotNull] this IMutableModel model)
            => (FirebirdModelAnnotations)Firebird((IModel)model);

        /// <summary>
        ///     Gets the SQL Server specific metadata for a model.
        /// </summary>
        /// <param name="model"> The model to get metadata for. </param>
        /// <returns> The SQL Server specific metadata for the model. </returns>
        public static IFirebirdModelAnnotations Firebird([NotNull] this IModel model)
            => new FirebirdModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
