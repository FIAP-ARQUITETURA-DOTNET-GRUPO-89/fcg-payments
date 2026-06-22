using MediatR;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.SharedKernel.Validators;
using OperationResult;

namespace FcgPayments.Application.Queries.Orders;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<GetOrderByIdResponse>>, IValidatableRequest
{
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; }
}
