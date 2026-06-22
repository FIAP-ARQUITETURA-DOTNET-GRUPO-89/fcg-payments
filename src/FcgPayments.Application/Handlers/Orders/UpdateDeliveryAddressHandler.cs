using MediatR;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Mappers;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using OperationResult;

namespace FcgPayments.Application.Handlers.Orders;

public sealed partial class UpdateDeliveryAddressHandler(
    IOrderRepository orderRepository,
    ILogger<UpdateDeliveryAddressHandler> logger)
: IRequestHandler<UpdateDeliveryAddressCommand, Result<UpdateDeliveryAddressResponse>>
{
    public async Task<Result<UpdateDeliveryAddressResponse>> Handle(UpdateDeliveryAddressCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
        {
            return new NotFoundException($"Pedido com ID {request.Id} não foi encontrado.");
        }

        if (!request.IsAdmin && order.Customer != request.UserId)
        {
            LogUnauthorizedAddressUpdateAttempt(logger, request.Id, request.UserId);

            return new NotFoundException($"Pedido com ID {request.Id} não foi encontrado.");
        }

        order.UpdateDeliveryAddress(request.Street, request.City, request.State, request.Cep);

        await orderRepository.SaveChangesAsync();

        LogDeliveryAddressUpdated(logger, request.Id, request.UserId);

        return Result.Success(order.ToUpdateDeliveryAddressResponse());
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "ALERTA: Usuário {UserId} tentou alterar o endereço do pedido {OrderId} que pertence a outro cliente.")]
    private static partial void LogUnauthorizedAddressUpdateAttempt(ILogger logger, Guid orderId, string? userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Endereço do pedido {OrderId} atualizado com sucesso pelo usuário {UserId}.")]
    private static partial void LogDeliveryAddressUpdated(ILogger logger, Guid orderId, string? userId);
}
