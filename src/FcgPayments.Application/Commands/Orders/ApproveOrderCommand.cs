using MediatR;
using OperationResult;

namespace FcgPayments.Application.Commands.Orders;

public sealed record ApproveOrderCommand(Guid OrderId): IRequest<Result>;
