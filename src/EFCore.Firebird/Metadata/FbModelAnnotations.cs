// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class FbModelAnnotations : RelationalModelAnnotations, IFirebirdModelAnnotations
    {
        public FbModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        protected FbModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual FirebirdValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (FirebirdValueGenerationStrategy?)Annotations.Metadata[FbAnnotationNames.ValueGenerationStrategy];

            set => SetValueGenerationStrategy(value);
        }

        protected virtual bool SetValueGenerationStrategy(FirebirdValueGenerationStrategy? value)
            => Annotations.SetAnnotation(FbAnnotationNames.ValueGenerationStrategy, value);
    }
}
