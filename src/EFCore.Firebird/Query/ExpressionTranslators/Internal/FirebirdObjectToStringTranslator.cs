// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FirebirdObjectToStringTranslator : IMethodCallTranslator
    {
        private const int DefaultLength = 100;

        private static readonly Dictionary<Type, string> _typeMapping
            = new Dictionary<Type, string>
            {
                { typeof(int), "CHAR(11)" },
                { typeof(long), "CHAR(20)" },
                { typeof(DateTime), $"CHAR({DefaultLength})" },
                { typeof(Guid), "CHAR(36)" },
                { typeof(bool), "CHAR(5)" },
                { typeof(byte), "CHAR(3)" },
                { typeof(byte[]), $"CHAR({DefaultLength})" },
                { typeof(double), $"CHAR({DefaultLength})" },
                { typeof(DateTimeOffset), $"CHAR({DefaultLength})" },
                { typeof(char), "CHAR(1)" },
                { typeof(short), "CHAR(6)" },
                { typeof(float), $"CHAR({DefaultLength})" },
                { typeof(decimal), $"CHAR({DefaultLength})" },
                { typeof(TimeSpan), $"CHAR({DefaultLength})" },
                { typeof(uint), "CHAR(10)" },
                { typeof(ushort), "CHAR(5)" },
                { typeof(ulong), "CHAR(19)" },
                { typeof(sbyte), "CHAR(4)" }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Debug.WriteLine("FirebirdObjectToStringTranslator.Translate " + methodCallExpression.Method.Name);
            
            string storeType;

            if (methodCallExpression.Method.Name == nameof(ToString)
                && methodCallExpression.Arguments.Count == 0
                && methodCallExpression.Object != null
                && _typeMapping.TryGetValue(
                    methodCallExpression.Object.Type
                        .UnwrapNullableType()
                        .UnwrapEnumType(),
                    out storeType))
            {
                return new SqlFunctionExpression(
                    functionName: "CAST",
                    returnType: methodCallExpression.Type,
                    arguments: new[]
                    {
                        methodCallExpression.Object,
                        new SqlFragmentExpression(storeType),
                    });
            }

            return null;
        }
    }
}
