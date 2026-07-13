using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.SharedKernel.Exceptions;
using Shouldly;

namespace FcgPayments.UnitTests.Domain.Entities;

public class PaymentTests
{
    private static Payment Create() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 199.90m);

    [Fact]
    public void Construtor_DeveCriarComStatusPendingEAmount()
    {
        // Act
        var payment = Create();

        // Assert
        payment.Status.ShouldBe(PaymentStatus.Pending);
        payment.Amount.ShouldBe(199.90m);
        payment.ProcessedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_AmountInvalido_LancaExcecao(decimal amount)
    {
        // Act & Assert
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), amount));
    }

    [Fact]
    public void Construtor_OrderIdVazio_LancaExcecao()
    {
        // Act & Assert
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 10m));
    }

    [Fact]
    public void Construtor_UserIdVazio_LancaExcecao()
    {
        // Act & Assert
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 10m));
    }

    [Fact]
    public void Construtor_GameIdVazio_LancaExcecao()
    {
        // Act & Assert
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 10m));
    }

    [Fact]
    public void Approve_QuandoPending_AtualizaStatusEDataProcessamento()
    {
        // Arrange
        var payment = Create();

        // Act
        payment.Approve();

        // Assert
        payment.Status.ShouldBe(PaymentStatus.Approved);
        payment.ProcessedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Reject_QuandoPending_AtualizaStatusEMotivo()
    {
        // Arrange
        var payment = Create();

        // Act
        payment.Reject("Saldo insuficiente");

        // Assert
        payment.Status.ShouldBe(PaymentStatus.Rejected);
        payment.Reason.ShouldBe("Saldo insuficiente");
    }

    [Fact]
    public void Reject_SemMotivo_LancaExcecao()
    {
        // Arrange
        var payment = Create();

        // Act & Assert
        Should.Throw<InvalidPaymentException>(() => payment.Reject("  "));
    }

    [Fact]
    public void Approve_QuandoJaProcessado_LancaExcecao()
    {
        // Arrange
        var payment = Create();
        payment.Approve();

        // Act & Assert
        Should.Throw<InvalidPaymentException>(() => payment.Approve());
    }
}
