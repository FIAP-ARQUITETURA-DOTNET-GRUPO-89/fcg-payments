using System.Net;
using FcgPayments.Application.Responses.Payments;
using FcgPayments.Domain.Enums;
using FcgPayments.IntegrationTests.Fixtures;
using FcgPayments.IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace FcgPayments.IntegrationTests.Payments;

[Collection("IntegrationTests")]
public class PaymentsApiTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetByOrderId_QuandoExiste_Retorna200ComDadosCorretos()
    {
        var ct = TestContext.Current.CancellationToken;

        var seeded = await fixture.ExecuteDbContextAsync(ctx =>
            ctx.Payments.AsNoTracking().FirstAsync(ct));

        var client = await TestAuthHelper.CreateAdminClientAsync(fixture);
        var response = await client.GetAsync($"/api/payments/{seeded.OrderId}", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.ReadContentAsync<PaymentResponse>(ct);
        body.ShouldNotBeNull();
        body!.OrderId.ShouldBe(seeded.OrderId);
        body.Amount.ShouldBe(seeded.Amount);
        body.Status.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public async Task GetByOrderId_QuandoNaoExiste_Retorna404()
    {
        var ct = TestContext.Current.CancellationToken;

        var client = await TestAuthHelper.CreateAdminClientAsync(fixture);
        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByOrderId_SemAutenticacao_Retorna401()
    {
        var ct = TestContext.Current.CancellationToken;

        var client = TestAuthHelper.CreateAnonymousClient(fixture);
        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByOrderId_ComRoleCustomer_Retorna403()
    {
        var ct = TestContext.Current.CancellationToken;

        var client = await TestAuthHelper.CreateUserCustomerAsync(fixture);
        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
