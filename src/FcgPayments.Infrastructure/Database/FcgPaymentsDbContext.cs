using FcgPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FcgPayments.Infrastructure.Database;

public class FcgPaymentsDbContext(DbContextOptions<FcgPaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcgPaymentsDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => configurationBuilder
            .Properties<string>()
            .AreUnicode(false)
            .HaveMaxLength(255);
}
