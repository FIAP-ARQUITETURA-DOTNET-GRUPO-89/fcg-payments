using FcgPayments.Domain.Entities;
using FcgPayments.Infrastructure.Repositories.Payments;
using FcgPayments.UnitTests.TestHelpers.Factories;
using Shouldly;

namespace FcgPayments.UnitTests.Infra.Repositories.Payments;

public class PaymentRepositoryTests
{
    [Fact]
    public async Task Add_PersistirEPodeBuscarPorOrderId()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new PaymentRepository(ctx);

        var orderId = Guid.NewGuid();
        var payment = new Payment(orderId, Guid.NewGuid(), Guid.NewGuid(), 99.90m);

        // Act
        repo.Add(payment);
        await repo.SaveChangesAsync(ct);

        var stored = await repo.GetByOrderIdAsync(orderId, ct);

        // Assert
        stored.ShouldNotBeNull();
        stored!.Id.ShouldBe(payment.Id);
    }

    [Fact]
    public async Task GetByOrderIdAsync_QuandoNaoExiste_RetornaNull()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new PaymentRepository(ctx);

        // Act
        var result = await repo.GetByOrderIdAsync(Guid.NewGuid(), ct);

        // Assert
        result.ShouldBeNull();
    }
}
