using FcgPayments.Domain.Enums;
using FcgPayments.SharedKernel.Exceptions;

namespace FcgPayments.Domain.Entities;

public class Payment : BaseEntity
{
    protected Payment() { }

    public Payment(Guid orderId, Guid userId, Guid gameId, decimal amount)
    {
        if (orderId == Guid.Empty)
        {
            throw new InvalidPaymentException("OrderId é obrigatório.");
        }

        if (userId == Guid.Empty)
        {
            throw new InvalidPaymentException("UserId é obrigatório.");
        }

        if (gameId == Guid.Empty)
        {
            throw new InvalidPaymentException("GameId é obrigatório.");
        }

        if (amount <= 0)
        {
            throw new InvalidPaymentException("O valor do pagamento deve ser maior que zero.");
        }

        OrderId = orderId;
        UserId = userId;
        GameId = gameId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Reason { get; private set; }

    public void Approve(string? reason = null)
    {
        EnsurePending();

        Status = PaymentStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
        Reason = reason;
        MarkAsUpdated();
    }

    public void Reject(string reason)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidPaymentException("Motivo da rejeição é obrigatório.");
        }

        Status = PaymentStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
        Reason = reason;
        MarkAsUpdated();
    }

    private void EnsurePending()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidPaymentException($"O pagamento já foi processado com o status '{Status}'.");
        }
    }
}
