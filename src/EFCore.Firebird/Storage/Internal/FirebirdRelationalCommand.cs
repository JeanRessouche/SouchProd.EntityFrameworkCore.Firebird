using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class FirebirdRelationalCommand : RelationalCommand
    {
		public FirebirdRelationalCommand(
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

        private RelationalDataReader Rdr;
        private WrappedFirebirdDataReader Wrp;
        private DbCommand dbCommand;

        private async Task<object> ExecuteAsync(
		    IOBehavior ioBehavior,
		    [NotNull] IRelationalConnection connection,
            DbCommandMethod executeMethod,
            [CanBeNull] IReadOnlyDictionary<string, object> parameterValues,
            CancellationToken cancellationToken = default(CancellationToken))
	    {
            Check.NotNull(connection, nameof(connection));

            /*using (DbCommand */
	        dbCommand = CreateCommand(connection, parameterValues);/*)*/
            {
                var fbConnection = connection as FirebirdRelationalConnection;
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

                   /* if (dbCommand.CommandText.ToUpper().Contains("INSERT ") && dbCommand.CommandText.ToUpper().Contains(" RETURNING "))
                    {
                        executeMethod = DbCommandMethod.ExecuteScalar;
                    }*/

                    switch (executeMethod)
                    {
                        case DbCommandMethod.ExecuteNonQuery:
                            {
                                result = ioBehavior == IOBehavior.Asynchronous ?
                                    await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) :
                                    dbCommand.ExecuteNonQuery();
                                break;
                            }
                        case DbCommandMethod.ExecuteScalar:
                            {
                                if (dbCommand.CommandText.ToUpper().Contains("INSERT ") &&
                                    dbCommand.CommandText.ToUpper().Contains(" RETURNING "))
                                {
                                    /*var lastInsertedInParam =
                                        new DbParameter("Id", FbDbType.BigInt);
                                    dbCommand.Parameters.Add(lastInsertedInParam);
                                    var idParam = dbCommand.Parameters["Id"];
                                    idParam.Direction = ParameterDirection.Output;
                                    Debug.WriteLine(idParam.Value.ToString());*/
                                    // dbCommand.CommandText
                                    // Parameters.Add("empID", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                                }

                                result = ioBehavior == IOBehavior.Asynchronous ?
                                    await dbCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) :
                                    dbCommand.ExecuteScalar();
                                break;
                            }
                        case DbCommandMethod.ExecuteReader:
                            {
                                var dataReader = ioBehavior == IOBehavior.Asynchronous ?
                                    await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false) :
                                    dbCommand.ExecuteReader();

                                Wrp = new WrappedFirebirdDataReader(dataReader);

                                Rdr = new RelationalDataReader(connection, dbCommand, Wrp, commandId, Logger);

                                result = Rdr;

                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException();
                            }
                    }

                    Logger.CommandExecuted(
                        dbCommand,
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
                        dbCommand,
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
                    dbCommand.Parameters.Clear();
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
                            Debug.WriteLine($"Parameter value = null");

                            parameterValue = DBNull.Value;
                            
                            TypeCode code = Type.GetTypeCode(parameterValue.GetType());

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
