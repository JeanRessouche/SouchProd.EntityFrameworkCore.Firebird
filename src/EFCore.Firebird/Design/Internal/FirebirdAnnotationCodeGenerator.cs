// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class FirebirdAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public FirebirdAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            return true;
        }

        public override string GenerateFluentApi(IIndex index, IAnnotation annotation, string language)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            return null;
        }
    }
}
