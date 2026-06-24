using FgcGames.EventContracts.Events;
using MassTransit;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Handlers.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Repositories.Orders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Handlers.Orders;

public class CreateOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly CreateOrderHandler _sut;

    public CreateOrderHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<CreateOrderHandler>>();

        _sut = new CreateOrderHandler(_orderRepository, _publishEndpoint, _logger);
    }

    [Fact]
    public async Task Dado_ComandoValido_Quando_CriarPedido_Entao_PersisteERetornaResposta()
    {
        // Arrange
        var command = CreateCommand();

        _orderRepository.SaveChangesAsync()
            .Returns(1);

        _publishEndpoint
            .Publish(Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldNotBe(Guid.Empty);

        _orderRepository.Received(1)
            .Add(Arg.Is<Order>(o =>
                o.Customer == command.Customer &&
                o.TotalAmount == command.TotalAmount &&
                o.DeliveryAddress.Street == command.Street &&
                o.DeliveryAddress.City == command.City &&
                o.DeliveryAddress.State.Equals(command.State, StringComparison.InvariantCultureIgnoreCase) &&
                o.DeliveryAddress.Cep == "88550000"));

        await _orderRepository.Received(1).SaveChangesAsync();

        await _publishEndpoint.Received(1)
                .Publish(
                    Arg.Is<OrderPlacedEvent>(e =>
                        e.Price == command.TotalAmount &&
                        e.OrderId != Guid.Empty),
                    Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dado_ErroAoPersistir_Quando_CriarPedido_Entao_PropagaExcecao()
    {
        // Arrange
        var command = CreateCommand();

        _orderRepository
            .SaveChangesAsync()
            .Returns(Task.FromException<int>(new InvalidOperationException("Database error")));

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.Handle(command, CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("Database error");

        _orderRepository.Received(1)
            .Add(Arg.Is<Order>(o =>
                o.Customer == command.Customer &&
                o.TotalAmount == command.TotalAmount &&
                o.DeliveryAddress.Street == command.Street &&
                o.DeliveryAddress.City == command.City &&
                o.DeliveryAddress.State.Equals(command.State, StringComparison.InvariantCultureIgnoreCase) &&
                o.DeliveryAddress.Cep == "88550000"));

        await _orderRepository.Received(1).SaveChangesAsync();

        await _publishEndpoint.DidNotReceive().Publish(Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dado_FalhaNoPublish_Quando_CriarPedido_Entao_PropagaExcecao()
    {
        // Arrange
        var command = CreateCommand();

        _orderRepository.SaveChangesAsync()
            .Returns(1);

        _publishEndpoint
            .Publish(Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new Exception("Erro no broker"));

        // Act
        var exception = await Should.ThrowAsync<Exception>(() =>
            _sut.Handle(command, CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("Erro no broker");

        _orderRepository.Received(1).Add(Arg.Any<Order>());

        await _orderRepository.Received(1).SaveChangesAsync();

        await _publishEndpoint.Received(1).Publish(Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }


    private static CreateOrderCommand CreateCommand()
        => new(
            Customer: "John Doe",
            TotalAmount: 150.75m,
            Street: "Main Street",
            City: "São Paulo",
            State: "sp",
            Cep: "88550-000");
}
