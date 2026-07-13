using FgcGames.EventContracts.Events;
using MassTransit;
using MediatR;
using FcgPayments.Application.Commands.Payments;

namespace FcgPayments.Worker.Consumers;

public sealed class OrderPlacedConsumer(
    IMediator mediator,
    ILogger<OrderPlacedConsumer> logger)
: IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        logger.LogInformation(
            "OrderPlaced recebido. OrderId: {OrderId}, Valor: {Price}.",
            context.Message.OrderId,
            context.Message.Price);

        var command = new ProcessPaymentCommand(
            context.Message.OrderId,
            context.Message.UserId,
            context.Message.GameId,
            context.Message.Price);

        await mediator.Send(command, context.CancellationToken);
    }
}
