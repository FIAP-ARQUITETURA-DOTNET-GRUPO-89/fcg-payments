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
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new PaymentRepository(ctx);

        var orderId = Guid.NewGuid();
        var payment = new Payment(orderId, Guid.NewGuid(), Guid.NewGuid(), 99.90m);

        repo.Add(payment);
        await repo.SaveChangesAsync();

        var stored = await repo.GetByOrderIdAsync(orderId);
        stored.ShouldNotBeNull();
        stored!.Id.ShouldBe(payment.Id);
    }

    [Fact]
    public async Task GetByOrderIdAsync_QuandoNaoExiste_RetornaNull()
    {
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new PaymentRepository(ctx);

        var result = await repo.GetByOrderIdAsync(Guid.NewGuid());

        result.ShouldBeNull();
    }
}
