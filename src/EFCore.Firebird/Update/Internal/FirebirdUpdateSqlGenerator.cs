// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using EFCore.Firebird.Utilities;
using Microsoft.EntityFrameworkCore.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public class FirebirdUpdateSqlGenerator : UpdateSqlGenerator, IFirebirdUpdateSqlGenerator
    {
        public FirebirdUpdateSqlGenerator(
            [NotNull] UpdateSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override ResultSetMapping AppendInsertOperation(
           StringBuilder commandStringBuilder,
           ModificationCommand command, 
           int commandPosition)
        {
            Check.NotNull(command, nameof(command));

            return AppendBulkInsertOperation(commandStringBuilder, new[] { command }, commandPosition);
        }


        public ResultSetMapping AppendBulkInsertOperation(StringBuilder commandStringBuilder, IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(modificationCommands, nameof(modificationCommands));

            var name = modificationCommands[0].TableName;
            var database = modificationCommands[0].Schema;
            var operations = modificationCommands[0].ColumnModifications;
            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            commandStringBuilder.Clear();
            commandStringBuilder.Append("EXECUTE BLOCK ");

            if (writeOperations.Any())
            {
                commandStringBuilder.AppendLine("(");
                var addSeparator = string.Empty;

                foreach (var t in modificationCommands)
                {
                    foreach (var column in t.ColumnModifications.Where(o => o.IsWrite))
                    {
                        commandStringBuilder
                            .Append(addSeparator)
                            .AppendLine($"{column.ParameterName}  {FirebirdSqlSqlGenerationHelper.GetTypeColumnToString(column)}=?");
                        addSeparator = ",";
                    }
                }

                commandStringBuilder
                    .AppendLine(")");
            }

            commandStringBuilder
                .AppendLine("RETURNS (LastInsertOrAffectedRows INT) AS BEGIN");

            foreach (ModificationCommand t in modificationCommands)
            {
                AppendInsertCommandHeader(commandStringBuilder, name, database, writeOperations);
                AppendValuesHeader(commandStringBuilder, t.ColumnModifications.Where(o => o.IsWrite).ToList());
                AppendValues(commandStringBuilder, t.ColumnModifications.Where(o => o.IsWrite).ToList());
                if (readOperations.Length > 0)
                    AppendInsertOutputClause(commandStringBuilder, readOperations, operations);
            }

            commandStringBuilder
                .AppendLine("END;")
                .Replace("@p", ":p");

            return ResultSetMapping.NotLastInResultSet;
        }

        public override ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            ModificationCommand command,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);

            AppendWhereClause(commandStringBuilder, conditionOperations);
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        public ResultSetMapping AppendBlockDeleteOperation(StringBuilder commandStringBuilder, IReadOnlyList<ModificationCommand> modificationCommands, int commandPosition)
        { 
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(modificationCommands, nameof(modificationCommands));
            var name = modificationCommands[0].TableName;

            commandStringBuilder
                .AppendLine("EXECUTE BLOCK RETURNS (LastInsertOrAffectedRows INT) AS BEGIN")
                .AppendLine("LastInsertOrAffectedRows = 0;");

            foreach (var t in modificationCommands)
            {
                var operations = t.ColumnModifications;

                commandStringBuilder
                    .Append($"DELETE FROM {SqlGenerationHelper.DelimitIdentifier(name)} ")
                    .AppendLine($" WHERE {SqlGenerationHelper.DelimitIdentifier(operations.First().ColumnName)}={operations[0].Value}; ");

                AppendUpdateOutputClause(commandStringBuilder);
            }

            commandStringBuilder
                .AppendLine("SUSPEND;")
                .AppendLine("END;");

            return ResultSetMapping.LastInResultSet;
        }

        private void AppendUpdateOutputClause(StringBuilder commandStringBuilder)
        {
            commandStringBuilder
                .AppendLine("IF (ROW_COUNT > 0) THEN")
                .AppendLine(" LastInsertOrAffectedRows = LastInsertOrAffectedRows + 1;");
        }

        private void AppendInsertOutputClause(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations,
            IReadOnlyList<ColumnModification> allOperations)
        {
            if (allOperations.Count > 0 && allOperations[0] == operations[0])
            {
                var key = SqlGenerationHelper.DelimitIdentifier(operations.First().ColumnName);
                commandStringBuilder
                    .AppendLine($"  RETURNING {key} INTO :LastInsertOrAffectedRows;")
                    .AppendLine("IF (ROW_COUNT > 0) THEN")
                    .AppendLine("   SUSPEND;");
            }
        }

        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name,
            string schema, int commandPosition)
        {
            return ResultSetMapping.LastInResultSet;
        }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            // TODO: Firebird
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            throw new NotImplementedException();
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder,
            int expectedRowsAffected)
        {
            //
        } 

        }
}
