using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.CartId).IsRequired();
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();

        // Configure Money value object for snapshotted price
        builder.ComplexProperty(i => i.SnapshotPrice, cp =>
        {
            cp.Property(m => m.Amount).IsRequired();
            cp.Property(m => m.Currency).IsRequired().HasMaxLength(3);
        });

        // Business invariant: a product can appear only once per cart
        // (quantity is incremented, not duplicated)
        builder.HasIndex(i => new { i.CartId, i.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_Cart_Product");
    }
}
