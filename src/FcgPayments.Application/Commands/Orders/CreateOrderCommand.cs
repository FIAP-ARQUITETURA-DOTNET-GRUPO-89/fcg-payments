using System.Text.Json.Serialization;
using MediatR;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.Domain.ValueObjects;
using FcgPayments.SharedKernel.Validators;
using OperationResult;

namespace FcgPayments.Application.Commands.Orders;

public record CreateOrderCommand(
    string Customer,
    decimal TotalAmount,
    string Street,
    string City,
    string State,
    string Cep)
: IRequest<Result<CreateOrderResponse>>, IValidatableRequest
{
    [JsonIgnore]
    public Address DeliveryAddress => new(Street, City, State, Cep);
}
