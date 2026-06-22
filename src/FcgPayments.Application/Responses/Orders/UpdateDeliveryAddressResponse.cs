namespace FcgPayments.Application.Responses.Orders;

public record UpdateDeliveryAddressResponse(
    Guid Id,
    string Customer,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Street,
    string City,
    string State,
    string Cep
);
