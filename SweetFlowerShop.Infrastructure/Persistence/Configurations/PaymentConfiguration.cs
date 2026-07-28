using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        // Scalar properties
        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        // Value Object: Money
        builder.OwnsOne(p => p.Amount, b => b.ConfigureMoney("Amount", "Currency"));

        // Navigation to child entities — explicit backing field access
        builder.HasMany(p => p.Transactions)
            .WithOne()
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One payment per order (business invariant)
        builder.HasIndex(p => p.OrderId).IsUnique();

        // Computed/transient properties
        builder.Ignore(p => p.TotalRefunded);
        builder.Ignore(p => p.DomainEvents);
    }
}
