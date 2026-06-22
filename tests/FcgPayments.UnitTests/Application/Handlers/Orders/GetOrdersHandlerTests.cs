using FcgPayments.Application.Handlers.Orders;
using FcgPayments.Application.Queries.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Repositories.Orders;
using NSubstitute;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Handlers.Orders;

public class GetOrdersHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly GetOrdersHandler _sut;

    public GetOrdersHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

        _sut = new GetOrdersHandler(_orderRepository);
    }

    [Fact]
    public async Task Dado_UsuarioComum_Quando_BuscarPedidos_Entao_FiltraPorUsuario()
    {
        // Arrange
        var query = CreateQuery(isAdmin: false, userId: "user-1");

        var orders = new List<Order>
        {
            CreateOrder("user-1"),
            CreateOrder("user-1")
        };

        _orderRepository
            .GetPagedAsNoTrackingAsync(query.Page, query.PageSize, "user-1", Arg.Any<CancellationToken>())
            .Returns((orders, orders.Count));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.ShouldNotBeNull();
        result.Value.Items.Count().ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.CurrentPage.ShouldBe(query.Page);
        result.Value.PageSize.ShouldBe(query.PageSize);

        await _orderRepository.Received(1).GetPagedAsNoTrackingAsync(query.Page, query.PageSize, "user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dado_Admin_Quando_BuscarPedidos_Entao_NaoFiltraPorUsuario()
    {
        // Arrange
        var query = CreateQuery(isAdmin: true, userId: "admin-1");

        var orders = new List<Order>
        {
            CreateOrder("user-1"),
            CreateOrder("user-2")
        };

        _orderRepository
            .GetPagedAsNoTrackingAsync(query.Page, query.PageSize, null, Arg.Any<CancellationToken>())
            .Returns((orders, orders.Count));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.ShouldNotBeNull();
        result.Value.Items.Count().ShouldBe(2);

        await _orderRepository.Received(1).GetPagedAsNoTrackingAsync(query.Page, query.PageSize, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dado_PaginacaoValida_Quando_BuscarPedidos_Entao_RetornaDadosCorretos()
    {
        // Arrange
        var query = CreateQuery(page: 2, pageSize: 5);

        var orders = new List<Order>
        {
            CreateOrder("user-1")
        };

        _orderRepository
            .GetPagedAsNoTrackingAsync(query.Page, query.PageSize, "user-1", Arg.Any<CancellationToken>())
            .Returns((orders, 12));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.ShouldNotBeNull();
        result.Value.TotalCount.ShouldBe(12);
        result.Value.CurrentPage.ShouldBe(2);
        result.Value.PageSize.ShouldBe(5);
        result.Value.TotalPages.ShouldBe(3);
        result.Value.HasNextPage.ShouldBeTrue();
        result.Value.HasPreviousPage.ShouldBeTrue();
    }

    private static GetOrdersQuery CreateQuery(
        int page = 1,
        int pageSize = 10,
        string? userId = "user-1",
        bool isAdmin = false)
        => new(page, pageSize)
        {
            UserId = userId,
            IsAdmin = isAdmin
        };

    private static Order CreateOrder(string customer)
        => new(
            customer: customer,
            totalAmount: 100,
            deliveryAddress: new("Street", "City", "SP", "88550000"));
}
