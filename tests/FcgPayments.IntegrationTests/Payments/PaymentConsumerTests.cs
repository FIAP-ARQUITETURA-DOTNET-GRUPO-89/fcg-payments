using FgcGames.EventContracts.Events;
using FcgPayments.Domain.Enums;
using FcgPayments.IntegrationTests.Fixtures;
using FcgPayments.IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace FcgPayments.IntegrationTests.Payments;

[Collection("IntegrationTests")]
public class PaymentConsumerTests(IntegrationTestFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Worker_QuandoRecebeOrderPlacedEvent_PersistePagamentoEPublicaResultado()
    {
        var ct = TestContext.Current.CancellationToken;
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        const decimal price = 199.90m;

        await fixture.Publisher.Publish(new OrderPlacedEvent(orderId, userId, gameId, price, DateTime.UtcNow), ct);

        var payment = await WaitUntil.ForAsync(
            fetchAction: () => fixture.ExecuteDbContextAsync(ctx =>
                ctx.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId, ct)),
            predicate: p => p is not null,
            timeout: TimeSpan.FromSeconds(30));

        payment.ShouldNotBeNull();
        payment!.OrderId.ShouldBe(orderId);
        payment.UserId.ShouldBe(userId);
        payment.GameId.ShouldBe(gameId);
        payment.Amount.ShouldBe(price);
        payment.Status.ShouldBe(PaymentStatus.Approved);
        payment.ProcessedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Worker_QuandoRecebeEventoDuplicado_NaoCriaPagamentoDuplicado()
    {
        var ct = TestContext.Current.CancellationToken;
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        const decimal price = 99.90m;

        var evento = new OrderPlacedEvent(orderId, userId, gameId, price, DateTime.UtcNow);

        await fixture.Publisher.Publish(evento, ct);

        await WaitUntil.ForAsync(
            fetchAction: () => fixture.ExecuteDbContextAsync(ctx =>
                ctx.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId, ct)),
            predicate: p => p is not null,
            timeout: TimeSpan.FromSeconds(30));

        await fixture.Publisher.Publish(evento, ct);

        await Task.Delay(2000, ct);

        var count = await fixture.ExecuteDbContextAsync(ctx =>
            ctx.Payments.CountAsync(p => p.OrderId == orderId, ct));

        count.ShouldBe(1);
    }
}
