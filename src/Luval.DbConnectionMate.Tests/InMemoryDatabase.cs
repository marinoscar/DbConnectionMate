using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.DbConnectionMate.Tests
{
    public class InMemoryDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public InMemoryDatabase()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
        }

        public async Task<IDbConnection> CreateDatabaseAsync()
        {
            string createTableQuery = @"
            CREATE TABLE Movies (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Director TEXT NULL,
                ReleaseYear INTEGER NULL
            );";

            using (var command = new SqliteCommand(createTableQuery, _connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            string insertDataQuery = @"
                INSERT INTO Movies (Title, Director, ReleaseYear) VALUES
                ('The Shawshank Redemption', 'Frank Darabont', 1994),
                ('The Godfather', 'Francis Ford Coppola', 1972),
                ('The Dark Knight', 'Christopher Nolan', 2008),
                ('Pulp Fiction', 'Quentin Tarantino', 1994),
                ('The Lord of the Rings: The Return of the King', 'Peter Jackson', 2003),
                ('Forrest Gump', 'Robert Zemeckis', 1994),
                ('Inception', 'Christopher Nolan', 2010),
                ('Fight Club', 'David Fincher', 1999),
                ('The Matrix', 'Lana Wachowski, Lilly Wachowski', 1999),
                ('Goodfellas', 'Martin Scorsese', 1990);";

            using (var command = new SqliteCommand(insertDataQuery, _connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            return _connection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
