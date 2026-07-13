using FgcGames.EventContracts.Events;
using MassTransit;
using FcgPayments.Application.Commands.Payments;
using FcgPayments.Application.Handlers.Payments;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.Domain.Repositories.Payments;
using FcgPayments.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using ContractStatus = FgcGames.EventContracts.Enums.PaymentStatus;

namespace FcgPayments.UnitTests.Application.Handlers.Payments;

public class ProcessPaymentHandlerTests
{
    private static ProcessPaymentCommand NewCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 199.90m);

    [Fact]
    public async Task Handle_AprovaPagamentoEPublicaEvento()
    {
        // Arrange
        var repo = Substitute.For<IPaymentRepository>();
        repo.GetByOrderIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var publisher = Substitute.For<IPublishEndpoint>();

        var strategy = Substitute.For<IPaymentApprovalStrategy>();
        strategy.ShouldApprove(Arg.Any<decimal>()).Returns(true);

        var handler = new ProcessPaymentHandler(repo, publisher, strategy, NullLogger<ProcessPaymentHandler>.Instance);
        var command = NewCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        repo.Received(1).Add(Arg.Is<Payment>(p => p.Status == PaymentStatus.Approved));
        await publisher.Received(1).Publish(
            Arg.Is<PaymentProcessedEvent>(e => e.OrderId == command.OrderId && e.Status == ContractStatus.Approved),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RejeitaPagamentoQuandoEstrategiaNaoAprova()
    {
        // Arrange
        var repo = Substitute.For<IPaymentRepository>();
        repo.GetByOrderIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var publisher = Substitute.For<IPublishEndpoint>();

        var strategy = Substitute.For<IPaymentApprovalStrategy>();
        strategy.ShouldApprove(Arg.Any<decimal>()).Returns(false);

        var handler = new ProcessPaymentHandler(repo, publisher, strategy, NullLogger<ProcessPaymentHandler>.Instance);
        var command = NewCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        repo.Received(1).Add(Arg.Is<Payment>(p => p.Status == PaymentStatus.Rejected));
        await publisher.Received(1).Publish(
            Arg.Is<PaymentProcessedEvent>(e => e.Status == ContractStatus.Rejected),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoPagamentoJaProcessado_NaoCriaDuplicado_RepublicaEvento()
    {
        // Arrange
        var command = NewCommand();
        var existing = new Payment(command.OrderId, command.UserId, command.GameId, command.Amount);
        existing.Approve();

        var repo = Substitute.For<IPaymentRepository>();
        repo.GetByOrderIdAsync(command.OrderId, Arg.Any<CancellationToken>()).Returns(existing);

        var publisher = Substitute.For<IPublishEndpoint>();
        var strategy = Substitute.For<IPaymentApprovalStrategy>();

        var handler = new ProcessPaymentHandler(repo, publisher, strategy, NullLogger<ProcessPaymentHandler>.Instance);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        repo.DidNotReceive().Add(Arg.Any<Payment>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await publisher.Received(1).Publish(
            Arg.Is<PaymentProcessedEvent>(e => e.OrderId == command.OrderId && e.Status == ContractStatus.Approved),
            Arg.Any<CancellationToken>());
    }
}
