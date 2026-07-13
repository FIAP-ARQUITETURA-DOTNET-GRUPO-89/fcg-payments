using MediatR;
using FcgPayments.SharedKernel.Validators;
using OperationResult;

namespace FcgPayments.Application.Commands.Payments;

public record ProcessPaymentCommand(Guid OrderId, Guid UserId, Guid GameId, decimal Amount)
: IRequest<Result<bool>>, IValidatableRequest;
