using FcgPayments.Domain.Entities;
using FcgPayments.Infrastructure.Database;

namespace FcgPayments.IntegrationTests.TestHelpers;

/// <summary>
/// Responsável por popular o banco de dados com dados iniciais necessários para os testes de integração.
/// </summary>
public static class TestDataSeeder
{
    public static async Task SeedAsync(FcgPaymentsDbContext context)
    {
        if (context.Payments.Any())
        {
            return;
        }

        var payment = new Payment(
            orderId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            gameId: Guid.NewGuid(),
            amount: 99.90m);

        context.Payments.Add(payment);

        await context.SaveChangesAsync();
    }
}
