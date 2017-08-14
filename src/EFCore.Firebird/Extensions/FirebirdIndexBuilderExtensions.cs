// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{

    public static class FirebirdIndexBuilderExtensions
    {
        public static IndexBuilder ForFirebirdIsFullText([NotNull] this IndexBuilder indexBuilder, bool fullText = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.Firebird().IsFullText = fullText;

            return indexBuilder;
        }

        public static IndexBuilder ForFirebirdIsSpatial([NotNull] this IndexBuilder indexBuilder, bool Spatial = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.Firebird().IsSpatial = Spatial;

            return indexBuilder;
        }
    }
}
