using FgcGames.EventContracts.Events;
using MassTransit;
using MediatR;
using FcgPayments.Application.Commands.Payments;
using FcgPayments.Domain.Services;
using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Enums;
using FcgPayments.Domain.Repositories.Payments;
using Microsoft.Extensions.Logging;
using OperationResult;
using ContractStatus = FgcGames.EventContracts.Enums.PaymentStatus;

namespace FcgPayments.Application.Handlers.Payments;

public sealed partial class ProcessPaymentHandler(
    IPaymentRepository repository,
    IPublishEndpoint publishEndpoint,
    IPaymentApprovalStrategy approvalStrategy,
    ILogger<ProcessPaymentHandler> logger)
: IRequestHandler<ProcessPaymentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null)
        {
            LogDuplicate(logger, request.OrderId, existing.Status);
            await PublishProcessedEvent(existing, cancellationToken);
            return Result.Success(true);
        }

        var payment = new Payment(request.OrderId, request.UserId, request.GameId, request.Amount);

        if (approvalStrategy.ShouldApprove(request.Amount))
        {
            payment.Approve();
        }
        else
        {
            payment.Reject("Pagamento recusado pela simulação.");
        }

        repository.Add(payment);
        await repository.SaveChangesAsync(cancellationToken);

        LogProcessed(logger, payment.OrderId, payment.Status);

        await PublishProcessedEvent(payment, cancellationToken);

        return Result.Success(true);
    }

    private Task PublishProcessedEvent(Payment payment, CancellationToken cancellationToken)
    {
        var contractStatus = payment.Status == PaymentStatus.Approved
            ? ContractStatus.Approved
            : ContractStatus.Rejected;

        var evt = new PaymentProcessedEvent(
            payment.OrderId,
            payment.UserId,
            payment.GameId,
            payment.Amount,
            contractStatus,
            payment.ProcessedAt ?? DateTime.UtcNow);

        return publishEndpoint.Publish(evt, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Pagamento processado. OrderId: {OrderId}, Status: {Status}.")]
    private static partial void LogProcessed(ILogger logger, Guid orderId, PaymentStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Pagamento duplicado ignorado. OrderId: {OrderId}, Status atual: {Status}.")]
    private static partial void LogDuplicate(ILogger logger, Guid orderId, PaymentStatus status);
}
