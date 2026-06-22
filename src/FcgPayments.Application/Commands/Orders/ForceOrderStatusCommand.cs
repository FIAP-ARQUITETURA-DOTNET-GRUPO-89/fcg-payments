using System.Text.Json.Serialization;
using MediatR;
using FcgPayments.Application.Responses.Orders;
using FcgPayments.SharedKernel.Validators;
using OperationResult;

namespace FcgPayments.Application.Commands.Orders;

public record ForceOrderStatusCommand
    : IRequest<Result<ForceOrderStatusResponse>>, IValidatableRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public bool IsAdmin { get; set; }

    public string NewStatus { get; set; } = string.Empty;
}
