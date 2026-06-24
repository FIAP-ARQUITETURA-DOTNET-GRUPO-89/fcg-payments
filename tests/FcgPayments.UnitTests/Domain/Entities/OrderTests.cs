using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.Domain.ValueObjects;
using FcgPayments.SharedKernel.Exceptions;
using Shouldly;

namespace FcgPayments.UnitTests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Dado_ComandoValido_Quando_CriarPedido_Entao_CriaComStatusPendingPayment()
    {
        // Arrange & Act
        var order = CreateValidOrder();

        // Assert
        order.Status.ShouldBe(OrderStatus.PendingPayment);
        order.Customer.ShouldBe("John Doe");
        order.TotalAmount.ShouldBe(100);
    }

    [Fact]
    public void Dado_ClienteVazio_Quando_CriarPedido_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => CreateOrder(customer: "");

        // Act & Assert
        Should.Throw<InvalidOrderException>(act);
    }

    [Fact]
    public void Dado_ClienteMuitoGrande_Quando_CriarPedido_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => CreateOrder(customer: new string('a', 101));

        // Act & Assert
        Should.Throw<InvalidOrderException>(act);
    }

    [Fact]
    public void Dado_ValorZero_Quando_CriarPedido_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => CreateOrder(totalAmount: 0);

        // Act & Assert
        Should.Throw<InvalidOrderException>(act);
    }

    [Fact]
    public void Dado_EnderecoNulo_Quando_CriarPedido_Entao_LancaExcecao()
    {
        // Arrange
        static void act() => new Order("John", 100, null!);

        // Act & Assert
        Should.Throw<MissingAddressException>(act);
    }

    [Fact]
    public void Dado_MesmoEndereco_Quando_AtualizarEndereco_Entao_NaoDeveAlterarNada()
    {
        // Arrange
        var order = CreateValidOrder();
        var enderecoOriginal = order.DeliveryAddress;

        // Act
        order.UpdateDeliveryAddress("Street", "City", "SP", "88550000");

        // Assert
        order.DeliveryAddress.ShouldBe(enderecoOriginal);
    }

    [Fact]
    public void Dado_PedidoComStatusDiferenteDePendingPayment_Quando_AtualizarEndereco_Entao_LancaOrderLockedException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Approve();

        // Act
        Action act = () => order.UpdateDeliveryAddress("New Street", "New City", "RJ", "12345678");

        // Assert
        Should.Throw<OrderLockedException>(act);
    }

    [Fact]
    public void Dado_PedidoValido_Quando_AtualizarEndereco_Entao_AtualizaEndereco()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.UpdateDeliveryAddress("New Street", "New City", "RJ", "12345678");

        // Assert
        order.DeliveryAddress.Street.ShouldBe("New Street");
        order.DeliveryAddress.City.ShouldBe("New City");
    }

    [Fact]
    public void Dado_PedidoEmAnalise_Quando_Aprovar_Entao_StatusMudaParaApproved()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.Approve();

        // Assert
        order.Status.ShouldBe(OrderStatus.Approved);
    }

    [Fact]
    public void Dado_PedidoEmAnalise_Quando_Rejeitar_Entao_StatusMudaParaRejected()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.Reject();

        // Assert
        order.Status.ShouldBe(OrderStatus.Rejected);
    }

    [Fact]
    public void Dado_PedidoEmAberto_Quando_Cancelar_Entao_StatusMudaParaRejected()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.Cancel();

        // Assert
        order.Status.ShouldBe(OrderStatus.Rejected);
    }

    [Fact]
    public void Dado_PedidoJaAprovado_Quando_AprovarNovamente_Entao_LancaInvalidOrderException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Approve();

        // Act
        Action act = () => order.Approve();

        // Assert
        Should.Throw<InvalidOrderException>(act);
    }

    [Fact]
    public void Dado_PedidoRejeitado_Quando_Cancelar_Entao_LancaExcecao()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Reject();

        Action act = () => order.Cancel();

        // Act & Assert
        Should.Throw<InvalidOrderException>(act);
    }

    [Fact]
    public void Dado_PedidoJaRejeitado_Quando_RejeitarNovamente_Entao_LancaInvalidOrderException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Reject();

        // Act
        Action act = () => order.Reject();

        // Assert
        Should.Throw<InvalidOrderException>(act);
    }

    [Fact]
    public void Dado_StatusDiferente_Quando_ForcarStatus_Entao_Atualiza()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.ForceStatus(OrderStatus.Approved);

        // Assert
        order.Status.ShouldBe(OrderStatus.Approved);
    }

    [Fact]
    public void Dado_StatusIgual_Quando_ForcarStatus_Entao_NaoAlteraNada()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.ForceStatus(OrderStatus.PendingPayment);

        // Assert
        order.Status.ShouldBe(OrderStatus.PendingPayment);
    }

    private static Order CreateValidOrder()
        => CreateOrder();

    private static Order CreateOrder(
        string customer = "John Doe",
        decimal totalAmount = 100,
        Address? address = null)
        => new(
            customer,
            totalAmount,
            address ?? new Address("Street", "City", "SP", "88550000"));
}
