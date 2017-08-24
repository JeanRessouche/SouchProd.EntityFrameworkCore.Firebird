// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class FirebirdIndexAnnotations : RelationalIndexAnnotations, IFirebirdIndexAnnotations
    {
        public FirebirdIndexAnnotations([NotNull] IIndex index)
            : base(index)
        {
        }

        protected FirebirdIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool? IsFullText
        {
            get => (bool?)Annotations.Metadata[FirebirdAnnotationNames.FullTextIndex];
           set => SetIsFullText(value);
        }

        protected virtual bool SetIsFullText(bool? value) => Annotations.SetAnnotation(
            FirebirdAnnotationNames.FullTextIndex,
            value);

        public virtual bool? IsSpatial
        {
            get => (bool?)Annotations.Metadata[FirebirdAnnotationNames.SpatialIndex];
             set => SetIsSpatial(value);
        }

        protected virtual bool SetIsSpatial(bool? value) => Annotations.SetAnnotation(
            FirebirdAnnotationNames.SpatialIndex,
            value);
    }
}
