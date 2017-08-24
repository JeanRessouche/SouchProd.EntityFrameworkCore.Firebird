// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FirebirdConvertTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToByte)] = "smallint",
            [nameof(Convert.ToDecimal)] = "NUMERIC(18, 2)",
            [nameof(Convert.ToDouble)] = "float",
            [nameof(Convert.ToInt16)] = "smallint",
            [nameof(Convert.ToInt32)] = "INTEGER",
            [nameof(Convert.ToInt64)] = "BIGINT",
            [nameof(Convert.ToString)] = "varchar(4000)"
        };

        private static readonly List<Type> _supportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        private static readonly IEnumerable<MethodInfo> _supportedMethods
            = _typeMapping.Keys
                .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(m => m.GetParameters().Length == 1
                                && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
         public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => (methodCallExpression.Method.Name == nameof(ToString)) ? 
            new FirebirdObjectToStringTranslator().Translate(methodCallExpression) :
            (_supportedMethods.Contains(methodCallExpression.Method)
                ? new SqlFunctionExpression(
                    "CONVERT",
                    methodCallExpression.Type,
                    new[]
                    {    
                        new SqlFragmentExpression(
                            _typeMapping[methodCallExpression.Method.Name]),
                        methodCallExpression.Arguments[0]
                    })
                : null);
    }
}
