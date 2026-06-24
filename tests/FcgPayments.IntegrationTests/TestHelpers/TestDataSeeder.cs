using FcgPayments.Domain.Entities;
using FcgPayments.Domain.ValueObjects;
using FcgPayments.Infrastructure.Database;

namespace FcgPayments.IntegrationTests.TestHelpers;

/// <summary>
/// Responsável por popular o banco de dados com dados iniciais necessários para os testes de integração.
/// </summary>
public static class TestDataSeeder
{
    public static async Task SeedAsync(FcgPaymentsDbContext context)
    {
        if (context.Orders.Any())
        {
            return;
        }

        var order = new Order(
            customer: "Cliente Teste",
            totalAmount: 150.00m,
            deliveryAddress: new Address(
                street: "Rua Teste",
                city: "Lages",
                state: "SC",
                cep: "88500000"
            )
        );

        context.Orders.Add(order);

        await context.SaveChangesAsync();
    }
}

