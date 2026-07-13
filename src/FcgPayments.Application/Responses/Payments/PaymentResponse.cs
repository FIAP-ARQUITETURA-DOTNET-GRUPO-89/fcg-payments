using FcgPayments.Domain.Enums;

namespace FcgPayments.Application.Responses.Payments;

public record PaymentResponse(
    Guid Id,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Amount,
    PaymentStatus Status,
    DateTime? ProcessedAt,
    string? Reason
);
