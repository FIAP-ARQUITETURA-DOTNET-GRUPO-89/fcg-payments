using FgcGames.EventContracts.Events;
using MassTransit;
using MediatR;
using FcgPayments.Application.Commands.Orders;

namespace FcgPayments.Worker.Consumers;

public sealed partial class OrderPlacedConsumer(
    IMediator mediator,
    ILogger<OrderPlacedConsumer> logger)
: IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        LogOrderReceived(logger, context.Message.OrderId, context.Message.Price);

        await mediator.Send(new ApproveOrderCommand(context.Message.OrderId));
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Pedido recebido. OrderId: {OrderId}, Valor: {Price}")]
    private static partial void LogOrderReceived(ILogger logger, Guid orderId, decimal price);
}
