using FcgPayments.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FcgPayments.UnitTests.TestHelpers.Factories;

public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Cria um DbContext com uma nova conexão SQLite em memória isolada.
    /// </summary>
    public static FcgPaymentsDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        return CreateContext(connection);
    }

    /// <summary>
    /// Cria um DbContext reaproveitando uma conexão SQLite existente (essencial para isolar asserts sem limpar a memória RAM).
    /// </summary>
    public static FcgPaymentsDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<FcgPaymentsDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new FcgPaymentsDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}
