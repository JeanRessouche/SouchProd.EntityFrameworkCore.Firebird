// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FirebirdSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        public FirebirdSqlGenerationHelper(
            [NotNull] RelationalSqlGenerationHelperDependencies dependencies,
            [NotNull] IFirebirdOptions options
            )
            : base(dependencies)
        {
            _options = options;
            _maxLength = _options.ConnectionSettings.ServerVersion.MetadataMaxLength;
        }

        private readonly IFirebirdOptions _options;

        private readonly int _maxLength;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            
            var initialLength = builder.Length;
            builder.Append(identifier.LimitLength(_maxLength));
            builder.Replace("\"", "\"\"", initialLength, identifier.Length);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string DelimitIdentifier(string identifier)
            => $"\"{EscapeIdentifier(Check.NotEmpty(identifier.LimitLength(_maxLength), nameof(identifier))).ToUpperInvariant()}\""; // Interpolation okay; strings
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            builder.Append('"');
            EscapeIdentifier(builder, identifier.LimitLength(_maxLength).ToUpperInvariant());
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
        {
            return "@" + name;
        }

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
        {
            builder.Append("@").Append(name);
        }
    }
}
