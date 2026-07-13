using MediatR;
using FcgPayments.Api.Extensions;
using FcgPayments.Application.Queries.Payments;
using FcgPayments.Application.Responses.Payments;
using Microsoft.AspNetCore.Mvc;

namespace FcgPayments.Api.Endpoints;

public static class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/payments")
            .WithTags("Pagamentos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{orderId:guid}", GetByOrderIdAsync)
            .RequireAuthorization("AdminPolicy")
            .WithSummary("Consulta o status de pagamento de um pedido")
            .WithDescription("Endpoint administrativo para diagnóstico do processamento simulado.")
            .Produces<PaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetByOrderIdAsync([FromRoute] Guid orderId, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
        return result.ToOkResult();
    }
}
