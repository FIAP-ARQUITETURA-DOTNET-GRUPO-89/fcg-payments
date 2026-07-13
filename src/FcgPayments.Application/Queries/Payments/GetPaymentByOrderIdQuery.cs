using MediatR;
using FcgPayments.Application.Responses.Payments;
using OperationResult;

namespace FcgPayments.Application.Queries.Payments;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<Result<PaymentResponse>>;
