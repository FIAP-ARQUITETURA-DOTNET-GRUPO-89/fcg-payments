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
        var payment = Create();

        payment.Status.ShouldBe(PaymentStatus.Pending);
        payment.Amount.ShouldBe(199.90m);
        payment.ProcessedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_AmountInvalido_LancaExcecao(decimal amount)
    {
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), amount));
    }

    [Fact]
    public void Construtor_OrderIdVazio_LancaExcecao()
    {
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 10m));
    }

    [Fact]
    public void Construtor_UserIdVazio_LancaExcecao()
    {
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 10m));
    }

    [Fact]
    public void Construtor_GameIdVazio_LancaExcecao()
    {
        Should.Throw<InvalidPaymentException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 10m));
    }

    [Fact]
    public void Approve_QuandoPending_AtualizaStatusEDataProcessamento()
    {
        var payment = Create();

        payment.Approve();

        payment.Status.ShouldBe(PaymentStatus.Approved);
        payment.ProcessedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Reject_QuandoPending_AtualizaStatusEMotivo()
    {
        var payment = Create();

        payment.Reject("Saldo insuficiente");

        payment.Status.ShouldBe(PaymentStatus.Rejected);
        payment.Reason.ShouldBe("Saldo insuficiente");
    }

    [Fact]
    public void Reject_SemMotivo_LancaExcecao()
    {
        var payment = Create();

        Should.Throw<InvalidPaymentException>(() => payment.Reject("  "));
    }

    [Fact]
    public void Approve_QuandoJaProcessado_LancaExcecao()
    {
        var payment = Create();
        payment.Approve();

        Should.Throw<InvalidPaymentException>(() => payment.Approve());
    }
}
