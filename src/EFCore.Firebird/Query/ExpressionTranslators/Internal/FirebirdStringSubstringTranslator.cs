// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FirebirdStringSubstringTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });

        /*

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!_methodInfo.Equals(methodCallExpression.Method))
                return null;

            var from = methodCallExpression.Arguments[0].NodeType == ExpressionType.Constant
                ? (Expression) Expression.Constant((int) ((ConstantExpression) methodCallExpression.Arguments[0]).Value + 1)
                : Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1));

            //var ex = new SqlFragmentExpression($"{methodCallExpression.Object} FROM {from} FOR {methodCallExpression.Arguments[1]}");
            
            return new SqlFunctionExpression(
                "SUBSTRING",
                methodCallExpression.Type,
                new[] { Expression.And(new SqlFragmentExpression($"{methodCallExpression.Object} FROM "),
                    Expression.AndAlso(from, new SqlFragmentExpression(" FOR "))
                    )
                });
        }*/

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!_methodInfo.Equals(methodCallExpression.Method))
                return null;

            var from = methodCallExpression.Arguments[0].NodeType == ExpressionType.Constant
                ? (Expression)Expression.Constant((int)((ConstantExpression)methodCallExpression.Arguments[0]).Value + 1)
                : Expression.Add(methodCallExpression.Arguments[0], Expression.Constant(1));

            return new SubStringExpression(
                    methodCallExpression.Object,
                    from,
                    methodCallExpression.Arguments[1]);
        } 
    }
}
