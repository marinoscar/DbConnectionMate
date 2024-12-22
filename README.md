# DbConnectionMate

**DbConnectionMate** is a lightweight and intuitive library that extends the `IDbConnection` interface, simplifying the execution of basic SQL commands in .NET applications. It encapsulates common tasks like opening connections, managing transactions, and handling parameters, allowing you to focus on your application's core logic.

## Key Features

- **Simplified SQL Execution**: Easily execute queries, commands, and stored procedures without repetitive boilerplate code.
- **Parameter Management**: Automatically handles parameters to make queries secure and concise.
- **Transaction Support**: Streamlines the use of database transactions for consistent and reliable data operations.
- **Code Clarity**: Reduces complexity in your database access code for better readability and maintainability.
- **Flexible and Extensible**: Works seamlessly with various ADO.NET providers and can be extended for custom use cases.

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package DbConnectionMate
```

## `ExecuteAsync` Method

### Summary
Executes a command asynchronously using the provided `IDbConnection`.

### Parameters
- `IDbConnection connection`: The database connection to use.
- `string? commandText`: The command text to execute.
- `IsolationLevel isolationLevel`: [Optional] The isolation level for the transaction. Default is `IsolationLevel.ReadCommitted`.
- `IEnumerable<IDbDataParameter>? parameters`: [Optional] The parameters to add to the command. Default is `null`.
- `CancellationToken cancellationToken`: [Optional] The cancellation token to cancel the operation. Default is `default`.

### Returns
A task representing the asynchronous operation, with the number of rows affected.

### Exceptions
- `ArgumentNullException`: Thrown when `commandText` is null.

### Example
```csharp
public class Example
{
    public async Task ExecuteSqlWithParamsAsync()
    {
        string connectionString = "your_connection_string"; 

        using (IDbConnection connection = new SqlConnection(connectionString))
        {
 
            var parameters = new List<SqlParameter> { new SqlParameter("@Name", "John Doe"), new SqlParameter("@Age", 30) };

            int rowsAffected = 
                await connection.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age);",
                parameters);

            Console.WriteLine($"Rows affected: {rowsAffected}");
        }
    }
}
```

## `ExecuteScalarAsync<T>` Method

### Summary
Executes a scalar command asynchronously using the provided `IDbConnection`.

### Type Parameters
- `T`: The type of the result.

### Parameters
- `IDbConnection connection`: The database connection to use.
- `string? commandText`: The command text to execute.
- `IsolationLevel isolationLevel`: [Optional] The isolation level for the transaction. Default is `IsolationLevel.ReadCommitted`.
- `IEnumerable<IDbDataParameter>? parameters`: [Optional] The parameters to add to the command. Default is `null`.
- `CancellationToken cancellationToken`: [Optional] The cancellation token to cancel the operation. Default is `default`.

### Returns
A task representing the asynchronous operation, with the result of the scalar command.

### Exceptions
- `ArgumentNullException`: Thrown when `commandText` is null.

### Example

```csharp
public class Example
{

    public async Task<DateTime> ExecuteScalarAsync()
    {
        string connectionString = "your_connection_string";
        using (IDbConnection connection = new SqlConnection(connectionString))
        {
            var result = DateTime.MinValue;
            result = await connection.ExecuteScalarAsync<DateTime>("SELECT GETDATE();");
            return result;
        }
    }
}
```

## `ExecuteReaderAsync` Method

### Summary
Executes a command asynchronously using the provided `IDbConnection` and processes the result with a data reader.

### Parameters
- `IDbConnection connection`: The database connection to use.
- `string? commandText`: The command text to execute.
- `IsolationLevel isolationLevel`: [Optional] The isolation level for the transaction. Default is `IsolationLevel.ReadCommitted`.
- `IEnumerable<IDbDataParameter>? parameters`: [Optional] The parameters to add to the command. Default is `null`.
- `CancellationToken cancellationToken`: [Optional] The cancellation token to cancel the operation. Default is `default`.

### Returns
A task representing the asynchronous operation, with the result being an enumerable of dictionaries where each dictionary represents a row with column names as keys and column values as values.

### Exceptions
- `ArgumentNullException`: Thrown when `commandText` is null.

### Example

```csharp
public class Example
{

    public async Task<List<Movie>> ExecuteReaderAsync()
    {
        string connectionString = "your_connection_string";
        using (IDbConnection conn = new SqlConnection(connectionString))
        {
            var result = new List<Movie>();
            var reader = await conn.ExecuteReaderAsync("SELECT Title, Director, ReleaseYear From Movies;");
            while (await reader.ReadAsync())
            {
                var movie = new Movie
                {
                    Title = reader["Title"].ToString(),
                    Director = reader["Director"].ToString(),
                    ReleaseYear = Convert.ToInt32(reader["ReleaseYear"])
                };
                result.Add(movie);
            }
            return result;
        }
    }
}

public class Movie
{
    public string Title { get; set; }
    public string Director { get; set; }
    public int ReleaseYear { get; set; }
}
```
