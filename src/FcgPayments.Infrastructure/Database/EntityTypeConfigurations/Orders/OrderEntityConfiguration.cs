using FcgPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FcgPayments.Infrastructure.Database.EntityTypeConfigurations.Orders;

public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Customer)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.ComplexProperty(p => p.DeliveryAddress, n =>
        {
            n.Property(e => e.Street).HasColumnName("Street").HasMaxLength(150).IsRequired();
            n.Property(e => e.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            n.Property(e => e.State).HasColumnName("State").HasMaxLength(2).IsRequired();
            n.Property(e => e.Cep).HasColumnName("Cep").HasMaxLength(8).IsRequired();
        });
    }
}
