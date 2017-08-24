// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FirebirdDateTimeNowTranslator : IMemberTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Expression == null
                && memberExpression.Member.DeclaringType == typeof(DateTime))
            {
                //switch (memberExpression.Member.Name)
                //{
                //    case nameof(DateTime.Now):
                        return new SqlFunctionExpression("CURRENT_TIMESTAMP", memberExpression.Type);
                // Not yet supported by FB
                // http://tracker.firebirdsql.org/browse/CORE-694
                //    case nameof(DateTime.UtcNow):
                //        return new SqlFunctionExpression("UTC_TIMESTAMP", memberExpression.Type);
                //}
            }

            return null;
        }
    }
}
