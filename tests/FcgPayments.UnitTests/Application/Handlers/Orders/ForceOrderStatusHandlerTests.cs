using Microsoft.Extensions.Logging;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Handlers.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Handlers.Orders;

public class ForceOrderStatusHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ForceOrderStatusHandler> _logger;
    private readonly ForceOrderStatusHandler _sut;

    public ForceOrderStatusHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<ForceOrderStatusHandler>>();

        _sut = new ForceOrderStatusHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Dado_PedidoExistente_Quando_ForcarStatus_Entao_AtualizaERetornaResposta()
    {
        // Arrange
        var command = CreateCommand(status: OrderStatus.Approved.ToString());

        var order = CreateOrder();

        _orderRepository
            .GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _orderRepository
            .SaveChangesAsync()
            .Returns(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        order.Status.ShouldBe(OrderStatus.Approved);

        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_PedidoNaoEncontrado_Quando_ForcarStatus_Entao_RetornaNotFound()
    {
        // Arrange
        var command = CreateCommand();

        _orderRepository
            .GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<NotFoundException>();

        await _orderRepository.Received(1)
            .GetByIdAsync(command.Id, Arg.Any<CancellationToken>());

        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_ErroAoPersistir_Quando_ForcarStatus_Entao_PropagaExcecao()
    {
        // Arrange
        var command = CreateCommand(status: OrderStatus.Rejected.ToString());

        var order = CreateOrder();

        _orderRepository
            .GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        _orderRepository
            .SaveChangesAsync()
            .Returns(Task.FromException<int>(
                new InvalidOperationException("Database error")));

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("Database error");

        await _orderRepository.Received(1).SaveChangesAsync();
    }

    private static ForceOrderStatusCommand CreateCommand(
        Guid? id = null,
        string status = "Approved",
        string? userId = "admin-1",
        bool isAdmin = true)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            NewStatus = status,
            UserId = userId,
            IsAdmin = isAdmin
        };

    private static Order CreateOrder()
        => new(
            customer: "John Doe",
            totalAmount: 100,
            deliveryAddress: new("Street", "City", "SP", "88550000"));
}
