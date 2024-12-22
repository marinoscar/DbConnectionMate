using Microsoft.Data.Sqlite;

namespace Luval.DbConnectionMate.Tests
{
    public class WithCommandAsync_UnitTests
    {
        [Fact]
        public async Task ExecutesCommandSuccesfully()
        {
            using (var conn = await new InMemoryDatabase().CreateDatabaseAsync())
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    await conn.ExecuteAsync("SELECT 1;");

                });
                Assert.Null(exception);
            }
        }

        [Fact]
        public async Task ExecuteAsync_ThrowsArgumentNullException_WhenCommandIsNull()
        {
            using (var conn = await new InMemoryDatabase().CreateDatabaseAsync())
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await conn.ExecuteAsync(null));
            }
        }

        [Fact]
        public async Task ExecuteAsync_WithParameters_Success()
        {
            using (var conn = await new InMemoryDatabase().CreateDatabaseAsync())
            {
                var parameters = new List<SqliteParameter> {
                    new SqliteParameter("@Title", "My Movie"),
                    new SqliteParameter("@Director", "The Director")
                };
                var exception = await Record.ExceptionAsync(async () =>
                {
                    await conn.ExecuteAsync("INSERT INTO Movies (Title, Director) VALUES (@Title, @Director);", parameters);
                });
                Assert.Null(exception);
            }
        }

        [Fact]
        public async Task ExecuteScalarAsync_ReturnsDateTimeValue()
        {
            using (var conn = await new InMemoryDatabase().CreateDatabaseAsync())
            {
                DateTime result = DateTime.MinValue;

                var exception = await Record.ExceptionAsync(async () =>
                {
                    result = await conn.ExecuteScalarAsync<DateTime>("SELECT datetime('now');");
                });

                Assert.Null(exception);
                Assert.IsType<DateTime>(result);
            }
        }

        [Fact]
        public async Task ExecuteReaderAsync_ReturnsDataReader()
        {
            using (var conn = await new InMemoryDatabase().CreateDatabaseAsync())
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    var result = await conn.ExecuteReaderAsync("SELECT * from Movies;");

                    Assert.NotNull(result);
                    Assert.True(result.Any());

                });

                Assert.Null(exception);
            }
        }







    }
}
