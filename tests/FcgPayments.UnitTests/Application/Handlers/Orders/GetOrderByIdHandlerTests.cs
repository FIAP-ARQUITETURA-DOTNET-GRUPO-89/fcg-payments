using FcgPayments.Application.Handlers.Orders;
using FcgPayments.Application.Queries.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Handlers.Orders;

public class GetOrderByIdHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrderByIdHandler> _logger;
    private readonly GetOrderByIdHandler _sut;

    public GetOrderByIdHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<GetOrderByIdHandler>>();

        _sut = new GetOrderByIdHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Dado_PedidoExistenteEUsuarioDono_Quando_BuscarPorId_Entao_RetornaPedido()
    {
        // Arrange
        var order = CreateOrder(customer: "user-1");

        var query = CreateQuery(order.Id, userId: "user-1", isAdmin: false);

        _orderRepository
            .GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(order.Id);
        result.Value.Customer.ShouldBe(order.Customer);

        await _orderRepository.Received(1).GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dado_PedidoNaoEncontrado_Quando_BuscarPorId_Entao_RetornaNotFound()
    {
        // Arrange
        var query = CreateQuery(Guid.NewGuid());

        _orderRepository
            .GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<NotFoundException>();

        await _orderRepository.Received(1).GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dado_PedidoDeOutroUsuario_Quando_BuscarPorId_Entao_RetornaNotFound()
    {
        // Arrange
        var order = CreateOrder(customer: "user-2");

        var query = CreateQuery(order.Id, userId: "user-1", isAdmin: false);

        _orderRepository
            .GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<NotFoundException>();

        await _orderRepository.Received(1).GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>());
        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_Admin_Quando_BuscarPedidoDeOutroUsuario_Entao_RetornaPedido()
    {
        // Arrange
        var order = CreateOrder(customer: "user-2");

        var query = CreateQuery(order.Id, userId: "admin-1", isAdmin: true);

        _orderRepository
            .GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(order.Id);

        await _orderRepository.Received(1).GetByIdAsNoTrackingAsync(query.Id, Arg.Any<CancellationToken>());
    }

    private static GetOrderByIdQuery CreateQuery(
        Guid? id = null,
        string? userId = "user-1",
        bool isAdmin = false)
        => new(id ?? Guid.NewGuid())
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
