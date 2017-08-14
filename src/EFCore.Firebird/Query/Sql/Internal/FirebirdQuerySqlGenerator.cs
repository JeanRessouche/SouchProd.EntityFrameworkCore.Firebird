/// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FirebirdQuerySqlGenerator : DefaultQuerySqlGenerator, IFirebirdExpressionVisitor
    {
        protected override string TypedTrueLiteral => "TRUE";
        protected override string TypedFalseLiteral => "FALSE";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FirebirdQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression)
            : base(dependencies, selectExpression)
        {
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

          if (selectExpression.Limit != null)
          {
              Sql.AppendLine().Append("FIRST ");
              Visit(selectExpression.Limit);
              Sql.AppendLine().Append(" ");
            }

          if (selectExpression.Offset != null)
          {
              if (selectExpression.Limit == null)
              {
                  // if we want to use Skip() without Take() we have to define the upper limit of LIMIT 
                  Sql.AppendLine().Append("FIRST ").Append(18446744073709551610).Append(" ");
              }
              Sql.Append(" SKIP ");
              Visit(selectExpression.Offset);
          }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression) { }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.FunctionName.StartsWith("@@", StringComparison.Ordinal))
            {
                Sql.Append(sqlFunctionExpression.FunctionName);

                return sqlFunctionExpression;
            }

            return base.VisitSqlFunction(sqlFunctionExpression);
        }

        protected override void GenerateProjection(Expression projection)
        {
            var aliasedProjection = projection as AliasExpression;
            var expressionToProcess = aliasedProjection?.Expression ?? projection;
            var updatedExperssion = ExplicitCastToBool(expressionToProcess);

            expressionToProcess = aliasedProjection != null
                ? new AliasExpression(aliasedProjection.Alias, updatedExperssion)
                : updatedExperssion;

            base.GenerateProjection(expressionToProcess);
        }

        private Expression ExplicitCastToBool(Expression expression)
        {
            return (expression as BinaryExpression)?.NodeType == ExpressionType.Coalesce
                   && expression.Type.UnwrapNullableType() == typeof(bool)
                ? new ExplicitCastExpression(expression, expression.Type)
                : expression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Add &&
                binaryExpression.Left.Type == typeof (string) &&
                binaryExpression.Right.Type == typeof (string))
            {
                Sql.Append("CONCAT(");
                //var exp = base.VisitBinary(binaryExpression);
                Visit(binaryExpression.Left);
                Sql.Append(",");
                var exp = Visit(binaryExpression.Right);
                Sql.Append(")");
                return exp;
            }
            
            var expr = base.VisitBinary(binaryExpression);
            
            return expr;
        }
        
        public virtual Expression VisitRegexp(RegexpExpression regexpExpression)
        {
            Check.NotNull(regexpExpression, nameof(regexpExpression));
            Visit(regexpExpression.Match);
            Sql.Append(" REGEXP ");
            Visit(regexpExpression.Pattern);
            return regexpExpression;
        }
    }
}
