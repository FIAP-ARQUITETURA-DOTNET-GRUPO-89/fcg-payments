using System.Net.Http.Headers;
using FcgPayments.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace FcgPayments.IntegrationTests.TestHelpers;

/// <summary>
/// Responsável por facilitar a criação de HttpClients autenticados para testes de integração.
/// </summary>
public static class TestAuthHelper
{
    public const string CustomerEmail = "user@fcgpayments.com";
    public const string AdminEmail = "admin@fcgpayments.com";

    /// <summary>
    /// Cria um HttpClient autenticado como usuário Admin.
    /// Realiza o login na API e configura automaticamente o header Authorization com um token JWT válido com permissões de administrador.
    /// </summary>
    public static async Task<HttpClient> CreateAdminClientAsync(IntegrationTestFixture fixture)
    {
        var client = fixture.CreateClient();

        var generator = fixture.App.Services.GetRequiredService<JwtTestTokenGenerator>();

        var token = generator.Generate(AdminEmail, "Admin");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// Cria um HttpClient autenticado como usuário (Customer).
    /// Realiza o login na API e configura automaticamente o header Authorization com um token JWT válido com permissões de usuário padrão.
    /// </summary>
    public static async Task<HttpClient> CreateUserCustomerAsync(IntegrationTestFixture fixture)
    {
        var client = fixture.CreateClient();

        var generator = fixture.App.Services.GetRequiredService<JwtTestTokenGenerator>();

        var token = generator.Generate(CustomerEmail, "Customer");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// Cria um HttpClient autenticado com uma role que não possui permissões na aplicação.
    /// Útil para validar cenários de autorização (403 Forbidden).
    /// </summary>
    public static async Task<HttpClient> CreateInvalidRoleClientAsync(IntegrationTestFixture fixture)
    {
        var client = fixture.CreateClient();

        var generator = fixture.App.Services.GetRequiredService<JwtTestTokenGenerator>();

        var token = generator.Generate("guest@fcgpayments.com", "Guest");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// Cria um HttpClient sem autenticação.
    /// Útil para testar cenários onde o acesso deve ser negado (401 Unauthorized) ou endpoints públicos que não exigem autenticação.
    /// </summary>
    public static HttpClient CreateAnonymousClient(IntegrationTestFixture fixture)
        => fixture.CreateClient();
}
