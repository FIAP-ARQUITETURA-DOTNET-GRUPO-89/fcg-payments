using MediatR;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.SharedKernel.Responses;
using FcgPayments.SharedKernel.Validators;
using OperationResult;

namespace FcgPayments.Application.Queries.Orders;

public record GetOrdersQuery(int Page = 1, int PageSize = 10)
    : IRequest<Result<PagedResponse<GetOrdersResponse>>>, IValidatableRequest
{
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; }
}
