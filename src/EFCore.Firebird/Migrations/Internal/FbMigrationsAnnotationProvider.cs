// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FbMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public FbMigrationsAnnotationProvider([NotNull] MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IModel model) => ForRemove(model);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IEntityType entityType) => ForRemove(entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            var isFullText = index.Firebird().IsFullText;
            if (isFullText.HasValue && isFullText.Value)
            {
                yield return new Annotation(
                    FbAnnotationNames.FullTextIndex,
                    "FULLTEXT");
            }

            var isSpatial = index.Firebird().IsSpatial;
            if (isSpatial.HasValue && isSpatial.Value)
            {
                yield return new Annotation(
                    FbAnnotationNames.SpatialIndex,
                    "SPATIAL");
            }

            foreach (var annotation in ForRemove(index))
            {
                yield return annotation;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IForeignKey foreignKey) => ForRemove(foreignKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.Firebird().ValueGenerationStrategy.HasValue)
            {
                yield return new Annotation(
                    FbAnnotationNames.ValueGenerationStrategy,
                    property.Firebird().ValueGenerationStrategy.Value);
            }

            foreach (var annotation in ForRemove(property))
            {
                yield return annotation;
            }
        }
    }
}
