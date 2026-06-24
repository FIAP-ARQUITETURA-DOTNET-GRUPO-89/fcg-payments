using FcgPayments.Domain.Entities;
using FcgPayments.Domain.Repositories.Orders;
using FcgPayments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FcgPayments.Infrastructure.Repositories.Orders;

public sealed class OrderRepository(FcgPaymentsDbContext context): IOrderRepository
{
    public void Add(Order order)
        => context.Orders.Add(order);
    public Task<Order?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Orders
            .FirstOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);

    public async Task<(IReadOnlyCollection<Order> Items, int TotalCount)> GetPagedAsNoTrackingAsync(
        int page,
        int pageSize,
        string? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(customerId))
        {
            query = query.Where(order => order.Customer == customerId);
        }

        query = query.OrderBy(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<int> SaveChangesAsync()
        => context.SaveChangesAsync();
}
