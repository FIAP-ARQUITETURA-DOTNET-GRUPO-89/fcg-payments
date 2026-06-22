using Microsoft.Extensions.Logging;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Handlers.Orders;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Handlers.Orders;

public class UpdateDeliveryAddressHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<UpdateDeliveryAddressHandler> _logger;
    private readonly UpdateDeliveryAddressHandler _sut;

    public UpdateDeliveryAddressHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<UpdateDeliveryAddressHandler>>();

        _sut = new UpdateDeliveryAddressHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Dado_PedidoExistenteEUsuarioDono_Quando_AtualizarEndereco_Entao_AtualizaERetornaResposta()
    {
        // Arrange
        var order = CreateOrder(customer: "user-1");

        var command = CreateCommand(order.Id, userId: "user-1", isAdmin: false);

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

        order.DeliveryAddress.Street.ShouldBe(command.Street);
        order.DeliveryAddress.City.ShouldBe(command.City);
        order.DeliveryAddress.State.ShouldBe(command.State.ToUpperInvariant());
        order.DeliveryAddress.Cep.ShouldBe("88550000");

        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_PedidoNaoEncontrado_Quando_AtualizarEndereco_Entao_RetornaNotFound()
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
    public async Task Dado_PedidoDeOutroUsuario_Quando_AtualizarEndereco_Entao_RetornaNotFound()
    {
        // Arrange
        var order = CreateOrder(customer: "user-2");

        var command = CreateCommand(order.Id, userId: "user-1", isAdmin: false);

        _orderRepository
            .GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Exception.ShouldBeOfType<NotFoundException>();

        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_Admin_Quando_AtualizarEnderecoDeOutroUsuario_Entao_AtualizaComSucesso()
    {
        // Arrange
        var order = CreateOrder(customer: "user-2");

        var command = CreateCommand(order.Id, userId: "admin-1", isAdmin: true);

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

        order.DeliveryAddress.Street.ShouldBe(command.Street);

        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Dado_ErroAoPersistir_Quando_AtualizarEndereco_Entao_PropagaExcecao()
    {
        // Arrange
        var order = CreateOrder();

        var command = CreateCommand(order.Id);

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

    private static UpdateDeliveryAddressCommand CreateCommand(
        Guid? id = null,
        string? userId = "user-1",
        bool isAdmin = false)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Street = "New Street",
            City = "New City",
            State = "sp",
            Cep = "88550-000",
            UserId = userId,
            IsAdmin = isAdmin
        };

    private static Order CreateOrder(string customer = "user-1")
        => new(
            customer: customer,
            totalAmount: 100,
            deliveryAddress: new("Street", "City", "SP", "88550000"));
}
