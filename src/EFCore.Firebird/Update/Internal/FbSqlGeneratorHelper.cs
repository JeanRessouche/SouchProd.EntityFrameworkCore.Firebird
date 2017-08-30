// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence)
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FbSqlSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        public FbSqlSqlGenerationHelper(
            [NotNull] RelationalSqlGenerationHelperDependencies dependencies
        )
            : base(dependencies)
        {

        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("\"", "\"\"");

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            var initialLength = builder.Length;
            builder.Append(identifier);
            builder.Replace("\"", "\"\"", initialLength, identifier.Length);
            //builder.Replace("", "", initialLength, identifier.Length);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string DelimitIdentifier(string identifier)
            => $"\"{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier))).ToUpperInvariant()}\""; // Interpolation okay; strings
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            builder.Append('"');
            EscapeIdentifier(builder, identifier.ToUpperInvariant());
            builder.Append('"');
        }

        //
        // Summary:
        //     Generates a valid parameter name for the given candidate name.
        //
        // Parameters:
        //   name:
        //     The candidate name for the parameter.
        //
        // Returns:
        //     A valid name based on the candidate name.
        public override string GenerateParameterName(string name)
            => $"@{name}";
        //
        // Summary:
        //     Writes a valid parameter name for the given candidate name.
        //
        // Parameters:
        //   builder:
        //     The System.Text.StringBuilder to write generated string to.
        //
        //   name:
        //     The candidate name for the parameter.
        public override void GenerateParameterName(StringBuilder builder, string name)
            => builder.Append("@").Append(name);

        public static object GenerateValue(ColumnModification column)
        {
            object value = null;
            if (column.Property.ClrType == typeof(string))
                value = ($"'{column.Value}'");
            else if (column.Property.ClrType == typeof(int)
                     || column.Property.GetType() == typeof(int?)
                     || column.Property.GetType() == typeof(long)
                     || column.Property.GetType() == typeof(long?)
            )
                value = (column.Value);
            else if (column.Property.ClrType == typeof(decimal)
                     || column.Property.GetType() == typeof(decimal?)
                     || column.Property.GetType() == typeof(double)
                     || column.Property.GetType() == typeof(double?)
            )
                value = (column.Value);
            else if (column.Property.ClrType == typeof(DateTime)
                     || column.Property.GetType() == typeof(DateTime?)
                     || column.Property.GetType() == typeof(TimeSpan)
                     || column.Property.GetType() == typeof(TimeSpan?)
            )
                value = ((column.Value == null ? null : $"'{DateTime.Parse(column.Value.ToString()).ToString("yyyy-MM-dd HH:mm:ss.ffff")}'"));

            else
                value = ($"'{column.Value}'");

            return value;
        }

        public static void GenerateValue(StringBuilder builder, ColumnModification column)
        {
            builder.Append(GenerateValue(column));
        }

        public static object GetTypeColumnToString(ColumnModification column, IRelationalTypeMapper mapper)
        {
            var mapping = mapper.FindMapping(column.Property);
            return mapping.StoreType;//"VARCHAR(100)";
        }

    }
}
