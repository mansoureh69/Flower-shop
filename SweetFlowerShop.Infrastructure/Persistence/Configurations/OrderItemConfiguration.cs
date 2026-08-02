using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        // Scalar properties — all required
        builder.Property(i => i.OrderId).IsRequired();
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(500);

        // Value Object: Money (owned type—stored as columns in OrderItems table)
        // UnitPrice is the Amount; Currency is stored alongside it
        builder.OwnsOne(i => i.UnitPrice, b => b.ConfigureMoney("UnitPrice", "UnitPrice_Currency"));

        // Index for "which orders contain this product?" queries
        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("IX_OrderItems_ProductId");

        // Computed/transient properties
        builder.Ignore(i => i.TotalPrice);
    }
}
