// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    
    public class SubStringExpression : Expression
    {
        
        public SubStringExpression([NotNull] Expression subjectExpression, [NotNull] Expression fromExpression, [NotNull] Expression forExpression)
        {
            Check.NotNull(subjectExpression, nameof(subjectExpression));
            Check.NotNull(fromExpression, nameof(fromExpression));
            Check.NotNull(forExpression, nameof(forExpression));

            SubjectExpression = subjectExpression;
            FromExpression = fromExpression;
            ForExpression = forExpression;
        }
        public virtual Expression SubjectExpression { get; }

        public virtual Expression FromExpression { get; }

        public virtual Expression ForExpression { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(string);

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as IFirebirdExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSubString(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newSubjectExpression = visitor.Visit(SubjectExpression);
            var newFromExpression = visitor.Visit(FromExpression);
            var newForExpression = visitor.Visit(ForExpression);

            return newFromExpression != FromExpression
                   || newForExpression != ForExpression
                   || newSubjectExpression != SubjectExpression
                ? new SubStringExpression(newSubjectExpression, newFromExpression, newForExpression)
                : this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((SubStringExpression)obj);
        }

        private bool Equals(SubStringExpression other)
            => Equals(FromExpression, other.FromExpression)
               && Equals(ForExpression, other.ForExpression) 
               && Equals(SubjectExpression, other.SubjectExpression) ;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SubjectExpression.GetHashCode();
                hashCode = (hashCode * 397) ^ FromExpression.GetHashCode();
                hashCode = (hashCode * 397) ^ ForExpression.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"SUBSTRING({SubjectExpression} FROM {FromExpression} FOR {ForExpression})";
        
    }
    
}
