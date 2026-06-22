using FcgPayments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;

namespace FcgPayments.IntegrationTests.TestHelpers;

/// <summary>
/// Responsável por gerenciar o ciclo de vida do banco de dados utilizado nos testes de integração.
/// </summary>
public class TestDatabaseManager(string connectionString)
{
    private readonly string _connectionString = connectionString;
    private Respawner _respawner = default!;

    /// <summary>
    /// Inicializa o banco de dados para uso nos testes.
    /// </summary>
    public async Task InitializeAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();

        await InitializeRespawner();
    }

    /// <summary>
    /// Restaura o banco de dados para um estado limpo.
    /// Remove todos os dados das tabelas configuradas e reaplica o seed inicial, garantindo que cada teste comece com um ambiente previsível e isolado.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await _respawner.ResetAsync(conn);

        await SeedAsync();
    }

    /// <summary>
    /// Configura o Respawner responsável por limpar o banco de dados.
    /// </summary>
    private async Task InitializeRespawner()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new("__EFMigrationsHistory")]
        });
    }

    /// <summary>
    /// Executa a carga inicial de dados no banco.
    /// </summary>
    private async Task SeedAsync()
    {
        await using var context = CreateDbContext();

        await TestDataSeeder.SeedAsync(context);
    }

    /// <summary>
    /// Cria uma instância do DbContext apontando para o banco de testes.
    /// </summary>
    private FcgPaymentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FcgPaymentsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new FcgPaymentsDbContext(options);
    }
}
