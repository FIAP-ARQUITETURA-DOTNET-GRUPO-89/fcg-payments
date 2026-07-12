using FcgPayments.Domain.Entities;

namespace FcgPayments.Domain.Repositories.Payments;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsNoTrackingAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
