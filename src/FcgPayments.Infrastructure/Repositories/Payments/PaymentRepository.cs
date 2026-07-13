using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Repositories.Payments;
using FcgPayments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FcgPayments.Infrastructure.Repositories.Payments;

public sealed class PaymentRepository(FcgPaymentsDbContext context) : IPaymentRepository
{
    public void Add(Payment payment) => context.Payments.Add(payment);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public Task<Payment?> GetByOrderIdAsNoTrackingAsync(Guid orderId, CancellationToken cancellationToken = default)
        => context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
