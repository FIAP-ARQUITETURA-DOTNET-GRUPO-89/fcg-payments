using MediatR;
using FcgPayments.Application.Queries.Payments;
using FcgPayments.Application.Responses.Payments;
using FcgPayments.Domain.Repositories.Payments;
using FcgPayments.SharedKernel.Exceptions;
using OperationResult;

namespace FcgPayments.Application.Handlers.Payments;

public sealed class GetPaymentByOrderIdHandler(IPaymentRepository repository)
: IRequestHandler<GetPaymentByOrderIdQuery, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByOrderIdAsNoTrackingAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Pagamento para o pedido {request.OrderId} não encontrado.");

        var response = new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.UserId,
            payment.GameId,
            payment.Amount,
            payment.Status,
            payment.ProcessedAt,
            payment.Reason);

        return Result.Success(response);
    }
}
