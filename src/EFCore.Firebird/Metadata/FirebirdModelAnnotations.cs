// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class FirebirdModelAnnotations : RelationalModelAnnotations, IFirebirdModelAnnotations
    {
        public FirebirdModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        protected FirebirdModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual FirebirdValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (FirebirdValueGenerationStrategy?)Annotations.Metadata[FirebirdAnnotationNames.ValueGenerationStrategy];

            set => SetValueGenerationStrategy(value);
        }

        protected virtual bool SetValueGenerationStrategy(FirebirdValueGenerationStrategy? value)
            => Annotations.SetAnnotation(FirebirdAnnotationNames.ValueGenerationStrategy, value);
    }
}
