// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{

    public class FbCommandBuilder : IRelationalCommandBuilder
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _logger;

        private readonly IndentedStringBuilder _commandTextBuilder = new IndentedStringBuilder();

        public FbCommandBuilder(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _logger = logger;
            ParameterBuilder = new RelationalParameterBuilder(typeMapper);
        }

        IndentedStringBuilder IInfrastructure<IndentedStringBuilder>.Instance
            => _commandTextBuilder;

        public virtual IRelationalParameterBuilder ParameterBuilder { get; }

        public virtual IRelationalCommand Build()
            => BuildCore(
                _logger,
                _commandTextBuilder.ToString(),
                ParameterBuilder.Parameters);

        protected virtual IRelationalCommand BuildCore(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<IRelationalParameter> parameters)
            => new FirebirdRelationalCommand(
                logger, commandText, parameters);

        public override string ToString() => _commandTextBuilder.ToString();
    }
}
