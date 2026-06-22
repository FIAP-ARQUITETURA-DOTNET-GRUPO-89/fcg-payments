using System.Security.Claims;
using MediatR;
using FcgPayments.Api.Extensions;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Queries.Orders;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.SharedKernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace FcgPayments.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/orders")
            .WithTags("Pedidos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateOrderAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Cria um novo pedido")
            .WithDescription("Processa a criação de um novo pedido no sistema.")
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetOrdersAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithName("GetOrders")
            .WithSummary("Lista pedidos de forma paginada")
            .WithDescription("Retorna os pedidos de forma paginada. Clientes visualizam apenas seus próprios pedidos, enquanto administradores possuem acesso global.")
            .Produces<GetOrdersResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetOrderByIdAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithName("GetOrderById")
            .WithSummary("Obtém um pedido por identificador")
            .WithDescription("Retorna os detalhes completos de um pedido específico.")
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateDeliveryAddressAsync)
            .RequireAuthorization("CustomerPolicy")
            .WithSummary("Atualiza o endereço de entrega")
            .WithDescription("Permite atualizar o endereço de entrega enquanto o pedido estiver aguardando pagamento.")
            .Produces<UpdateDeliveryAddressResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/force-status", ForceOrderStatusAsync)
            .RequireAuthorization("AdminPolicy")
            .WithName("ForceOrderStatus")
            .WithSummary("Força a alteração de status de um pedido")
            .WithDescription("Operação de contingência destinada exclusivamente a administradores para correção manual de status.")
            .Produces<ForceOrderStatusResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateOrderAsync([FromBody] CreateOrderCommand command, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.ToCreatedResult(x => $"/api/orders/{result.Value!.Id}");
    }

    private static async Task<IResult> GetOrdersAsync([AsParameters] PaginationRequest request, ClaimsPrincipal user, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery(request.Page, request.PageSize)
        {
            UserId = user.FindFirst(ClaimTypes.Name)?.Value,
            IsAdmin = user.IsInRole("Admin")
        };

        var result = await mediator.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetOrderByIdAsync([FromRoute] Guid id, ClaimsPrincipal user, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id)
        {
            UserId = user.FindFirst(ClaimTypes.Name)?.Value,
            IsAdmin = user.IsInRole("Admin")
        };

        var result = await mediator.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> UpdateDeliveryAddressAsync([FromRoute] Guid id, [FromBody] UpdateDeliveryAddressCommand command, ClaimsPrincipal user, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        command.Id = id;

        command.UserId = user.FindFirst(ClaimTypes.Name)?.Value;
        command.IsAdmin = user.IsInRole("Admin");

        var result = await mediator.Send(command, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> ForceOrderStatusAsync([FromRoute] Guid id, [FromBody] ForceOrderStatusCommand command, ClaimsPrincipal user, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        command.Id = id;

        command.UserId = user.FindFirst(ClaimTypes.Name)?.Value;
        command.IsAdmin = user.IsInRole("Admin");

        var result = await mediator.Send(command, cancellationToken);

        return result.ToOkResult();
    }
}
