namespace FcgPayments.Application.Responses.Orders;

public record GetOrdersResponse(
    Guid Id,
    string Customer,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt
);
