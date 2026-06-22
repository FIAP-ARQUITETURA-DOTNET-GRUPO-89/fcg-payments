using MediatR;
using FcgPayments.Application.Mappers;
using FcgPayments.Application.Queries.Orders;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.SharedKernel.Responses;
using OperationResult;

namespace FcgPayments.Application.Handlers.Orders;

public sealed class GetOrdersHandler(
    IOrderRepository orderRepository)
: IRequestHandler<GetOrdersQuery, Result<PagedResponse<GetOrdersResponse>>>
{
    public async Task<Result<PagedResponse<GetOrdersResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var customerId = request.IsAdmin
            ? null
            : request.UserId;

        var (items, totalCount) = await orderRepository.GetPagedAsNoTrackingAsync(
            request.Page,
            request.PageSize,
            customerId,
            cancellationToken);

        var pagedResponse = new PagedResponse<GetOrdersResponse>(items.ToGetOrdersResponseList(), totalCount, request.Page, request.PageSize);

        return Result.Success(pagedResponse);
    }
}
