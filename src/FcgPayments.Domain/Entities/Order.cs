using FcgPayments.Domain.Enums;
using FcgPayments.Domain.ValueObjects;
using FcgPayments.SharedKernel.Exceptions;

namespace FcgPayments.Domain.Entities;

/// <summary>
/// Representa um pedido realizado por um cliente.
/// </summary>
public class Order : BaseEntity
{
    protected Order() { }

    public Order(string customer, decimal totalAmount, Address deliveryAddress)
    {
        if (string.IsNullOrWhiteSpace(customer))
        {
            throw new InvalidOrderException("O nome do cliente é obrigatório.");
        }

        if (customer.Length > 100)
        {
            throw new InvalidOrderException("O nome do cliente deve possuir no máximo 100 caracteres.");
        }

        if (totalAmount <= 0)
        {
            throw new InvalidOrderException("O valor total do pedido deve ser maior que zero.");
        }

        if (deliveryAddress is null)
        {
            throw new MissingAddressException("O endereço de entrega é obrigatório para a criação do pedido.");
        }

        Customer = customer.Trim();
        TotalAmount = totalAmount;
        DeliveryAddress = deliveryAddress;

        Status = OrderStatus.PendingPayment;
    }

    /// <summary>
    /// Nome do cliente responsável pelo pedido.
    /// </summary>
    public string Customer { get; private set; } = null!;

    /// <summary>
    /// Valor total do pedido.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Status atual do pedido.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Endereço de entrega associado ao pedido.
    /// </summary>
    public Address DeliveryAddress { get; private set; } = null!;

    /// <summary>
    /// Atualiza o endereço de entrega do pedido.
    /// </summary>
    public void UpdateDeliveryAddress(string street, string city, string state, string cep)
    {
        EnsureEditable();

        var newAddress = new Address(street, city, state, cep);

        if (DeliveryAddress == newAddress)
        {
            return;
        }

        DeliveryAddress = newAddress;

        MarkAsUpdated();
    }

    /// <summary>
    /// Aprova o pedido.
    /// </summary>
    public void Approve()
    {
        EnsureStatus(OrderStatus.PendingPayment);

        Status = OrderStatus.Approved;

        MarkAsUpdated();
    }

    /// <summary>
    /// Rejeita o pedido.
    /// </summary>
    public void Reject()
    {
        EnsureStatus(OrderStatus.PendingPayment);

        Status = OrderStatus.Rejected;

        MarkAsUpdated();
    }

    /// <summary>
    /// Cancela o pedido.
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Rejected)
        {
            throw new InvalidOrderException("Um pedido rejeitado não pode ser cancelado.");
        }

        Status = OrderStatus.Rejected;

        MarkAsUpdated();
    }

    /// <summary>
    /// Operação administrativa de contingência.
    /// </summary>
    public void ForceStatus(OrderStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        Status = newStatus;

        MarkAsUpdated();
    }

    private void EnsureEditable()
    {
        if (Status != OrderStatus.PendingPayment)
        {
            throw new OrderLockedException($"Não é possível alterar o endereço de um pedido com o status atual: {Status}.");
        }
    }

    private void EnsureStatus(OrderStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOrderException($"O pedido deve estar com status '{expectedStatus}' para executar esta operação.");
        }
    }
}
