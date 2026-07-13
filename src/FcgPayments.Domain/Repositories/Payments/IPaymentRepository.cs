using FcgPayments.Domain.Entities;

namespace FcgPayments.Domain.Repositories.Payments;

public interface IPaymentRepository
{
    /// <summary>
    /// Adiciona um novo pagamento ao contexto de persistência.
    /// </summary>
    void Add(Payment payment);

    /// <summary>
    /// Retorna o pagamento associado ao pedido informado, com rastreamento de alterações pelo EF Core.
    /// Retorna <c>null</c> se nenhum pagamento for encontrado para o <paramref name="orderId"/>.
    /// </summary>
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o pagamento associado ao pedido informado sem rastreamento de alterações pelo EF Core.
    /// Preferível para operações de leitura onde o objeto não será modificado.
    /// Retorna <c>null</c> se nenhum pagamento for encontrado para o <paramref name="orderId"/>.
    /// </summary>
    Task<Payment?> GetByOrderIdAsNoTrackingAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste todas as alterações pendentes no banco de dados.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
