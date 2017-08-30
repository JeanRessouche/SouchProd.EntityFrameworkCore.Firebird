// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FbCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new FbContainsOptimizedTranslator(),
            new FbConvertTranslator(),
            new FbDateAddTranslator(),
            new FbEndsWithOptimizedTranslator(),
            new FbMathTranslator(),
            new FbNewGuidTranslator(),
            new FbObjectToStringTranslator(),
            new FbStartsWithOptimizedTranslator(),
            new FbStringIsNullOrWhiteSpaceTranslator(),
            new FbStringReplaceTranslator(),
            new FbStringSubstringTranslator(),
            new FbStringToLowerTranslator(),
            new FbStringToUpperTranslator(),
            new FbStringTrimEndTranslator(),
            new FbStringTrimStartTranslator(),
            new FbStringTrimTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FbCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
