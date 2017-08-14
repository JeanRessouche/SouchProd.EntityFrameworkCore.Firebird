/// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    public interface IFirebirdExpressionVisitor
    {
        Expression VisitRegexp([NotNull] RegexpExpression regexpExpression);
    }
}
