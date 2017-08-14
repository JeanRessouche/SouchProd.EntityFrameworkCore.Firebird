// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class FirebirdKeyAnnotations : RelationalKeyAnnotations, IFirebirdKeyAnnotations
    {
        public FirebirdKeyAnnotations([NotNull] IKey key)
            : base(key)
        {
        }

        protected FirebirdKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }
    }
}
