using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FbRelationalCommand : RelationalCommand
    {
		public FbRelationalCommand(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] string commandText,
            [NotNull] IReadOnlyList<IRelationalParameter> parameters)
			: base(logger, commandText, parameters)
        {
        }

        protected override object Execute(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues)
        {
		    return ExecuteAsync(IOBehavior.Synchronous, connection, executeMethod, parameterValues)
			    .GetAwaiter()
			    .GetResult();
	    }

	    protected override async Task<object> ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default(CancellationToken))
        {
		    return await ExecuteAsync(IOBehavior.Asynchronous, connection, executeMethod, parameterValues, cancellationToken).ConfigureAwait(false);
	    }

        private RelationalDataReader _rdr;
        private WrappedFirebirdDataReader _wrp;
        private DbCommand _dbCommand;

        private async Task<object> ExecuteAsync(
		    IOBehavior ioBehavior,
		    [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default(CancellationToken))
	    {
            Check.NotNull(connection, nameof(connection));

            /*using (DbCommand */
	        _dbCommand = CreateCommand(connection, parameterValues);/*)*/
            {
                var fbConnection = connection as FbRelationalConnection;
                object result;
                var opened = false;
                var commandId = Guid.NewGuid();
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    if (ioBehavior == IOBehavior.Asynchronous)
                        // ReSharper disable once PossibleNullReferenceException
                        await fbConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    else
                        // ReSharper disable once PossibleNullReferenceException
                        fbConnection.Open();

                    opened = true;

                    if (_dbCommand.CommandText.ToUpper().Contains("INSERT ") && _dbCommand.CommandText.ToUpper().Contains(" RETURNING "))
                        executeMethod = DbCommandMethod.ExecuteScalar;

                    switch (executeMethod)
                    {
                        case DbCommandMethod.ExecuteNonQuery:
                            {
                                result = ioBehavior == IOBehavior.Asynchronous ?
                                    await _dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) :
                                    _dbCommand.ExecuteNonQuery();
                                break;
                            }
                        case DbCommandMethod.ExecuteScalar:
                            {
                                result = ioBehavior == IOBehavior.Asynchronous ?
                                    await _dbCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) :
                                    _dbCommand.ExecuteScalar();
                                break;
                            }
                        case DbCommandMethod.ExecuteReader:
                            {
                                var dataReader = ioBehavior == IOBehavior.Asynchronous ?
                                    await _dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false) :
                                    _dbCommand.ExecuteReader();

                                _wrp = new WrappedFirebirdDataReader(dataReader);
                                _rdr = new RelationalDataReader(connection, _dbCommand, _wrp, commandId, Logger);

                                result = _rdr;
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException();
                            }
                    }

                    Logger.CommandExecuted(
                        _dbCommand,
                        executeMethod,
                        commandId,
                        connection.ConnectionId,
                        result,
                        ioBehavior == IOBehavior.Asynchronous,
                        startTime,
                        stopwatch.Elapsed);

                }
                catch (Exception exception)
                {
                    Logger.CommandError(
                        _dbCommand,
                        executeMethod,
                        commandId,
                        connection.ConnectionId,
                        exception,
                        ioBehavior == IOBehavior.Asynchronous,
                        startTime,
                        stopwatch.Elapsed);

                    if (opened)
                        connection.Close();

                    throw;
                }
                finally
                {
                    _dbCommand.Parameters.Clear();
                }

                return result;
            }
        }

        private DbCommand CreateCommand(
            IRelationalConnection connection,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            var command = connection.DbConnection.CreateCommand();

            command.CommandText = CommandText;

            if (connection.CurrentTransaction != null)
            {
                command.Transaction = connection.CurrentTransaction.GetDbTransaction();
            }

            if (connection.CommandTimeout != null)
            {
                command.CommandTimeout = (int) connection.CommandTimeout;
            }

            if (Parameters.Count > 0)
            {
                if (parameterValues == null)
                {
                    throw new InvalidOperationException(RelationalStrings.MissingParameterValue(Parameters[0].InvariantName));
                }

                foreach (var parameter in Parameters)
                {
                    object parameterValue;
               
                    Debug.WriteLine($"Set parameter {parameter.InvariantName}");

                    if (parameterValues.TryGetValue(parameter.InvariantName, out parameterValue))
                    {
                        if (parameterValue != null)
	                    {
                            Debug.WriteLine($"Parameter value = {parameterValue}");
                            if (parameterValue is bool)
                                parameter.AddDbParameter(command, Convert.ToInt16((bool)parameterValue));
                            else if (parameterValue is char)
			                    parameter.AddDbParameter(command, Convert.ToByte((char)parameterValue));
		                    else if (parameterValue.GetType().FullName.StartsWith("System.JsonObject"))
			                    parameter.AddDbParameter(command, parameterValue.ToString());
							else if (parameterValue.GetType().GetTypeInfo().IsEnum)
                                parameter.AddDbParameter(command, Convert.ChangeType(parameterValue, Enum.GetUnderlyingType(parameterValue.GetType())));
		                    else
			                    parameter.AddDbParameter(command, parameterValue);
	                    }
	                    else
	                    {
                            // Parameter value is null, this is not supported by the FB Provider 5.9.1.
                            // as the TypeCode is not supported in .Net Standard 1.6.
                            // SO, let's fork the FB Provider project, porting it to 2.0 & send a PR.
                            parameter.AddDbParameter(command, parameterValue);
	                    }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.MissingParameterValue(parameter.InvariantName));
                    }
                }
            }

            return command;
        }

    }
}
