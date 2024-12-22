using System.Data;
using System.Data.Common;

namespace Luval.DbConnectionMate
{
    public static class IDbConnectionExtensions
    {
        #region Private Methods

        private static async Task OpenConnectionAsync(IDbConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Closed)
            {
                if (connection is DbConnection dbConn)
                {
                    await dbConn.OpenAsync(cancellationToken);
                }
                else
                {
                    await Task.Run(() => connection.Open(), cancellationToken);
                }
            }
        }

        private static async Task<IDbTransaction> BeingTransactionAsync(IDbConnection connection, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            await OpenConnectionAsync(connection, cancellationToken);
            if (connection is DbConnection dbConn)
            {
                return await dbConn.BeginTransactionAsync(isolationLevel, cancellationToken);
            }
            else
            {
                return await Task.Run(() => connection.BeginTransaction(isolationLevel), cancellationToken);
            }
        }

        #endregion

        #region WithCommandAsync

        public static Task WithCommandAsync(this IDbConnection connection, Action<IDbCommand> withCommand, string? commandText, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (withCommand == null) throw new ArgumentNullException(nameof(withCommand));
            return WithCommandAsync(connection, commandText, withCommand, IsolationLevel.ReadCommitted, parameters, cancellationToken);
        }

        public static Task WithCommandAsync(this IDbConnection connection, Action<IDbCommand> withCommand, string? commandText, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (withCommand == null) throw new ArgumentNullException(nameof(withCommand));
            return WithCommandAsync(connection, commandText, withCommand, isolationLevel, null, cancellationToken);
        }

        public static Task WithCommandAsync(this IDbConnection connection, Action<IDbCommand> withCommand, string? commandText, CancellationToken cancellationToken = default)
        {
            if (withCommand == null) throw new ArgumentNullException(nameof(withCommand));
            return WithCommandAsync(connection, commandText, withCommand, IsolationLevel.ReadCommitted, null, cancellationToken);
        }

        public static async Task WithCommandAsync(this IDbConnection connection, string? commandText, Action<IDbCommand> withCommand, IsolationLevel isolationLevel, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            if (withCommand == null) throw new ArgumentNullException(nameof(withCommand));

            using (connection)
            {
                await OpenConnectionAsync(connection, cancellationToken);
                using (var command = connection.CreateCommand())
                {
                    if (parameters != null && parameters.Any())
                    {
                        foreach (var p in parameters)
                        {
                            command.Parameters.Add(p);
                        }
                    }
                    command.CommandType = CommandType.Text;
                    command.CommandText = commandText;
                    command.Transaction = await BeingTransactionAsync(connection, isolationLevel, cancellationToken);
                    try
                    {
                        withCommand(command);
                        await Task.Run(command.Transaction.Commit, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        if (connection.State == ConnectionState.Open) await Task.Run(command.Transaction.Rollback, cancellationToken);
                        throw new Exception("Failed to run command", ex);
                    }
                }
            }
        }

        #endregion

        #region ExecuteAsync

        public static async Task<int> ExecuteAsync(this IDbConnection connection, string? commandText, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteAsync(connection, commandText, IsolationLevel.ReadCommitted, parameters, cancellationToken);
        }

        public static async Task<int> ExecuteAsync(this IDbConnection connection, string? commandText, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteAsync(connection, commandText, isolationLevel, null, cancellationToken);
        }

        public static async Task<int> ExecuteAsync(this IDbConnection connection, string? commandText, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteAsync(connection, commandText, IsolationLevel.ReadCommitted, null, cancellationToken);
        }

        public static async Task<int> ExecuteAsync(this IDbConnection connection, string? commandText, IsolationLevel isolationLevel, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));

            int result = 0;
            var withCommand = new Action<IDbCommand>(async (c) =>
            {
                result = await Task.Run(() => c.ExecuteNonQuery(), cancellationToken);
            });
            await WithCommandAsync(connection, commandText, withCommand, isolationLevel, parameters, cancellationToken);
            return result;
        }

        #endregion

        #region ExecuteScalarAsync

        public static async Task<T?> ExecuteScalarAsync<T>(this IDbConnection connection, string? commandText, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteScalarAsync<T>(connection, commandText, IsolationLevel.ReadCommitted, parameters, cancellationToken);
        }

        public static async Task<T?> ExecuteScalarAsync<T>(this IDbConnection connection, string? commandText, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteScalarAsync<T>(connection, commandText, isolationLevel, null, cancellationToken);
        }

        public static async Task<T?> ExecuteScalarAsync<T>(this IDbConnection connection, string? commandText, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteScalarAsync<T>(connection, commandText, IsolationLevel.ReadCommitted, null, cancellationToken);
        }

        public static async Task<T?> ExecuteScalarAsync<T>(this IDbConnection connection, string? commandText, IsolationLevel isolationLevel, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));

            object? result = default;
            var withCommand = new Action<IDbCommand>(async (c) =>
            {
                result = await Task.Run(() => c.ExecuteScalar(), cancellationToken);
            });
            await WithCommandAsync(connection, commandText, withCommand, isolationLevel, parameters, cancellationToken);
            if (DBNull.Value.Equals(result) || result == null) return default(T);
            return (T)result;
        }

        #endregion

        #region WithDataReaderAsync

        public static async Task WithDataReaderAsync(this IDbConnection connection, string? commandText, Action<IDataReader> withReader, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            if (withReader == null) throw new ArgumentNullException(nameof(withReader));
            await WithDataReaderAsync(connection, commandText, withReader, IsolationLevel.ReadCommitted, null, cancellationToken);
        }

        public static async Task WithDataReaderAsync(this IDbConnection connection, string? commandText, Action<IDataReader> withReader, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            if (withReader == null) throw new ArgumentNullException(nameof(withReader));
            await WithDataReaderAsync(connection, commandText, withReader, isolationLevel, null, cancellationToken);
        }

        public static async Task WithDataReaderAsync(this IDbConnection connection, string? commandText, Action<IDataReader> withReader, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            if (withReader == null) throw new ArgumentNullException(nameof(withReader));
            await WithDataReaderAsync(connection, commandText, withReader, IsolationLevel.ReadCommitted, parameters, cancellationToken);
        }

        public static async Task WithDataReaderAsync(this IDbConnection connection, string? commandText, Action<IDataReader> withReader, IsolationLevel isolationLevel, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            if (withReader == null) throw new ArgumentNullException(nameof(withReader));
            await WithCommandAsync(connection, commandText, (command) =>
            {
                using (var reader = command.ExecuteReader())
                {
                    withReader(reader);
                }
            }
            , isolationLevel, parameters, cancellationToken);
        }

        #endregion

        #region ExecuteReaderAsync

        public static async Task<IEnumerable<IDictionary<string, object>>> ExecuteReaderAsync(this IDbConnection connection, string? commandText, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteReaderAsync(connection, commandText, IsolationLevel.ReadCommitted, null, cancellationToken);
        }

        public static async Task<IEnumerable<IDictionary<string, object>>> ExecuteReaderAsync(this IDbConnection connection, string? commandText, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteReaderAsync(connection, commandText, IsolationLevel.ReadCommitted, parameters, cancellationToken);
        }

        public static async Task<IEnumerable<IDictionary<string, object>>> ExecuteReaderAsync(this IDbConnection connection, string? commandText, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));
            return await ExecuteReaderAsync(connection, commandText, isolationLevel, null, cancellationToken);
        }

        public static async Task<IEnumerable<IDictionary<string, object>>> ExecuteReaderAsync(this IDbConnection connection, string? commandText, IsolationLevel isolationLevel, IEnumerable<IDbDataParameter>? parameters, CancellationToken cancellationToken = default)
        {
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));

            var result = new List<IDictionary<string, object>>();
            var withReader = new Action<IDataReader>((reader) =>
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    result.Add(row);
                }
            });
            await WithDataReaderAsync(connection, commandText, withReader, isolationLevel, parameters, cancellationToken);
            return result;
        }

        #endregion
    }
}
