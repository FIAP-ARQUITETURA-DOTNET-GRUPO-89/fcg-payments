using Microsoft.Extensions.Logging;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Handlers.Orders;

public class ApproveOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ApproveOrderHandler> _logger;
    private readonly ApproveOrderHandler _sut;

    public ApproveOrderHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<ApproveOrderHandler>>();

        _sut = new ApproveOrderHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Dado_PedidoExistente_Quando_AprovarPedido_Entao_AtualizaStatusParaApprovedERetornaSucesso()
    {
        // Arrange
        var command = CreateCommand();
        var order = CreateOrder();

        _orderRepository
            .GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
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
    public async Task Dado_PedidoNaoEncontrado_Quando_AprovarPedido_Entao_RetornaNotFoundException()
    {
        // Arrange
        var command = CreateCommand();

        _orderRepository
            .GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<NotFoundException>();
        result.Exception.Message.ShouldBe($"Pedido {command.OrderId} não encontrado.");

        await _orderRepository.Received(1).GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>());

        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_ErroAoPersistir_Quando_AprovarPedido_Entao_PropagaExcecao()
    {
        // Arrange
        var command = CreateCommand();
        var order = CreateOrder();

        _orderRepository
            .GetByIdAsync(command.OrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        _orderRepository
            .SaveChangesAsync()
            .Returns(Task.FromException<int>(new InvalidOperationException("Erro de persistência no banco de dados.")));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Erro de persistência no banco de dados.");

        await _orderRepository.Received(1).SaveChangesAsync();
    }

    private static ApproveOrderCommand CreateCommand(Guid? orderId = null)
        => new(OrderId: orderId ?? Guid.NewGuid());

    private static Order CreateOrder()
        => new(
            customer: "Cliente Teste",
            totalAmount: 150.00m,
            deliveryAddress: new("Rua Teste", "Lages", "SC", "88500000"));
}
