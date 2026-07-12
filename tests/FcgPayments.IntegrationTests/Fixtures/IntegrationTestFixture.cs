using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FcgPayments.Infrastructure.Database;
using FcgPayments.IntegrationTests.TestHelpers;
using FcgPayments.SharedKernel.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FcgPayments.IntegrationTests.Fixtures;

/// <summary>
/// Fixture base para testes de integração da aplicação.
/// Essa classe atua como ponto central de orquestração da infraestrutura de testes.
/// </summary>
public class IntegrationTestFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = default!;
    public IBus Publisher { get; private set; } = default!;

    private IBusControl _busControl = default!;
    private TestDatabaseManager _dbManager = default!;
    private string _connectionString = string.Empty;

    /// <summary>
    /// Inicializa o ambiente de testes.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.FcgPayments_AppHost>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.AddSingleton<JwtTestTokenGenerator>();

        builder.Services.ConfigureHttpClientDefaults(client =>
        {
            client.ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
        });

        App = await builder.BuildAsync();
        await App.StartAsync();

        _connectionString = await App.GetConnectionStringAsync("Default") ?? throw new InvalidOperationException("Connection string não encontrada");
        _dbManager = new TestDatabaseManager(_connectionString);
        await _dbManager.InitializeAsync();
        await _dbManager.ResetAsync();

        var rabbitMqConnStr = await App.GetConnectionStringAsync("rabbitmq")
            ?? throw new InvalidOperationException("RabbitMQ connection string não encontrada");

        _busControl = Bus.Factory.CreateUsingRabbitMq(cfg => cfg.Host(new Uri(rabbitMqConnStr)));
        await _busControl.StartAsync();
        Publisher = _busControl;
    }

    /// <summary>
    /// Finaliza a execução da aplicação após os testes.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_busControl is not null)
        {
            await _busControl.StopAsync();
        }

        if (App is not null)
        {
            await App.StopAsync();
            await App.DisposeAsync();
        }
    }

    /// <summary>
    /// Reseta o banco de dados para um estado limpo.
    /// </summary>
    public async Task ResetDatabaseAsync()
        => await _dbManager.ResetAsync();

    /// <summary>
    /// Cria um HttpClient configurado para comunicação com a API.
    /// </summary>
    public HttpClient CreateClient()
        => App.CreateHttpClient("fcgpayments-api", endpointName: "https");

    /// <summary>
    /// Executa uma função isolada utilizando um <see cref="FcgPaymentsDbContext"/> apontando para o banco de testes.
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<FcgPaymentsDbContext, Task<T>> action)
    {
        var options = new DbContextOptionsBuilder<FcgPaymentsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var context = new FcgPaymentsDbContext(options);
        return await action(context);
    }
}
