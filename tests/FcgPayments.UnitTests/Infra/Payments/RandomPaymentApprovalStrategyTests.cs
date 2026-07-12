using FcgPayments.Infrastructure.Payments;
using FcgPayments.SharedKernel.Settings;
using Microsoft.Extensions.Options;
using Shouldly;

namespace FcgPayments.UnitTests.Infra.Payments;

public class RandomPaymentApprovalStrategyTests
{
    private static RandomPaymentApprovalStrategy Create(double rate)
        => new(Options.Create(new PaymentSimulationSettings { ApprovalRate = rate }));

    [Fact]
    public void ShouldApprove_QuandoApprovalRate1_SempreAprova()
    {
        var strategy = Create(1.0);

        strategy.ShouldApprove(199.90m).ShouldBeTrue();
        strategy.ShouldApprove(0.01m).ShouldBeTrue();
        strategy.ShouldApprove(9999.99m).ShouldBeTrue();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRateAcimaDe1_SempreAprova()
    {
        var strategy = Create(1.5);

        strategy.ShouldApprove(100m).ShouldBeTrue();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRate0_SempreRejeita()
    {
        var strategy = Create(0.0);

        strategy.ShouldApprove(199.90m).ShouldBeFalse();
        strategy.ShouldApprove(0.01m).ShouldBeFalse();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRateAbaixoDe0_SempreRejeita()
    {
        var strategy = Create(-0.5);

        strategy.ShouldApprove(100m).ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ShouldApprove_QuandoAmountInvalido_SempreRejeita(decimal amount)
    {
        var strategy = Create(1.0);

        strategy.ShouldApprove(amount).ShouldBeFalse();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRateIntermediaria_RetornaBoolean()
    {
        var strategy = Create(0.5);

        const int iterations = 1000;
        var approvals = Enumerable.Range(0, iterations)
            .Count(_ => strategy.ShouldApprove(100m));

        approvals.ShouldBeGreaterThan(300);
        approvals.ShouldBeLessThan(700);
    }
}
