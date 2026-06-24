using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.Domain.ValueObjects;
using FcgPayments.Infrastructure.Repositories.Orders;
using FcgPayments.UnitTests.TestHelpers.Factories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace FcgPayments.UnitTests.Infra.Repositories.Orders;

public class OrderRepositoryTests
{
    [Fact]
    public async Task Dado_PedidoValido_Quando_Adicionar_Entao_DevePersistir()
    {
        // Arrange
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var context = InMemoryDbContextFactory.CreateContext(connection);
        var repository = new OrderRepository(context);
        var order = CreateOrder("John Doe");

        // Act
        repository.Add(order);
        await repository.SaveChangesAsync();

        // Assert
        using var assertContext = InMemoryDbContextFactory.CreateContext(connection);

        var result = await assertContext.Orders.FindAsync([order.Id], TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Customer.ShouldBe("John Doe");
        result.TotalAmount.ShouldBe(100);
    }

    [Fact]
    public async Task Dado_PedidoExistente_Quando_BuscarPorId_Entao_DeveRetornarPedidoRastreado()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        var order = CreateOrder("Maria");
        context.Orders.Add(order);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(order.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Customer.ShouldBe("Maria");
    }

    [Fact]
    public async Task Dado_PedidoInexistente_Quando_BuscarPorId_Entao_DeveRetornarNulo()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Dado_PedidoExistente_Quando_BuscarPorIdAsNoTracking_Entao_DeveRetornarPedidoNaoRastreado()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        var order = CreateOrder("Lucas");
        context.Orders.Add(order);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsNoTrackingAsync(order.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Customer.ShouldBe("Lucas");

        context.Entry(result).State.ShouldBe(EntityState.Detached);
    }

    [Fact]
    public async Task Dado_PedidoInexistente_Quando_BuscarPorIdAsNoTracking_Entao_DeveRetornarNulo()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsNoTrackingAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Dado_MultiplosPedidos_Quando_BuscarPaginado_Entao_DeveRetornarPaginaCorreta()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        context.Orders.AddRange(
            CreateOrder("Ana"),
            CreateOrder("Bruno"),
            CreateOrder("Carlos"),
            CreateOrder("Daniel")
        );

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new OrderRepository(context);

        // Act
        var (items, totalCount) = await repository.GetPagedAsNoTrackingAsync(page: 2, pageSize: 2, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        items.Count.ShouldBe(2);
        totalCount.ShouldBe(4);
        items.First().Customer.ShouldBe("Carlos");
    }

    [Fact]
    public async Task Dado_FiltroPorCliente_Quando_BuscarPaginado_Entao_DeveFiltrarCorretamente()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        context.Orders.AddRange(
            CreateOrder("Ana"),
            CreateOrder("Bruno"),
            CreateOrder("Bruno"),
            CreateOrder("Carlos")
        );

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new OrderRepository(context);

        // Act
        var (items, totalCount) = await repository.GetPagedAsNoTrackingAsync(page: 1, pageSize: 10, customerId: "Bruno", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        items.Count.ShouldBe(2);
        totalCount.ShouldBe(2);
        items.All(x => x.Customer == "Bruno").ShouldBeTrue();
    }

    [Fact]
    public async Task Dado_PaginaMaiorQueExistente_Quando_BuscarPaginado_Entao_DeveRetornarListaVaziaETotalCountCorreto()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateContext();
        context.Orders.AddRange(CreateOrder("Ana"), CreateOrder("Bruno"));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new OrderRepository(context);

        // Act
        var (items, totalCount) = await repository.GetPagedAsNoTrackingAsync(page: 5, pageSize: 2, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        items.ShouldBeEmpty();
        totalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Dado_PedidoExistente_Quando_AtualizarESalvar_Entao_DevePersistirAlteracao()
    {
        // Arrange
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var context = InMemoryDbContextFactory.CreateContext(connection);
        var order = CreateOrder("Original");
        context.Orders.Add(order);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new OrderRepository(context);
        var orderToUpdate = await repository.GetByIdAsync(order.Id, TestContext.Current.CancellationToken);

        // Act
        orderToUpdate!.ForceStatus(OrderStatus.Approved);
        await repository.SaveChangesAsync();

        // Assert
        using var assertContext = InMemoryDbContextFactory.CreateContext(connection);
        var result = await assertContext.Orders.FindAsync([order.Id], TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(OrderStatus.Approved);
    }

    private static Order CreateOrder(string customer)
        => new(
            customer,
            100,
            new Address("Street 1", "City", "SP", "88550000"));
}
