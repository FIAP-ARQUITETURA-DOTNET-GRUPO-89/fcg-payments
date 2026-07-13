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
        // Arrange
        var strategy = Create(1.0);

        // Act & Assert
        strategy.ShouldApprove(199.90m).ShouldBeTrue();
        strategy.ShouldApprove(0.01m).ShouldBeTrue();
        strategy.ShouldApprove(9999.99m).ShouldBeTrue();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRateAcimaDe1_SempreAprova()
    {
        // Arrange
        var strategy = Create(1.5);

        // Act & Assert
        strategy.ShouldApprove(100m).ShouldBeTrue();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRate0_SempreRejeita()
    {
        // Arrange
        var strategy = Create(0.0);

        // Act & Assert
        strategy.ShouldApprove(199.90m).ShouldBeFalse();
        strategy.ShouldApprove(0.01m).ShouldBeFalse();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRateAbaixoDe0_SempreRejeita()
    {
        // Arrange
        var strategy = Create(-0.5);

        // Act & Assert
        strategy.ShouldApprove(100m).ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ShouldApprove_QuandoAmountInvalido_SempreRejeita(decimal amount)
    {
        // Arrange
        var strategy = Create(1.0);

        // Act & Assert
        strategy.ShouldApprove(amount).ShouldBeFalse();
    }

    [Fact]
    public void ShouldApprove_QuandoApprovalRateIntermediaria_RetornaBoolean()
    {
        // Arrange
        var strategy = Create(0.5);
        const int iterations = 1000;

        // Act
        var approvals = Enumerable.Range(0, iterations)
            .Count(_ => strategy.ShouldApprove(100m));

        // Assert
        approvals.ShouldBeGreaterThan(300);
        approvals.ShouldBeLessThan(700);
    }
}
