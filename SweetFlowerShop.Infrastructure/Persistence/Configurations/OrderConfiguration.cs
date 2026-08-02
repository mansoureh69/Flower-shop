using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        // Scalar properties
        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.OrderDate).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(1000);

        // Value Object: DeliveryInfo (nullable owned type — order may not have delivery info yet)
        builder.OwnsOne(o => o.DeliveryInfo, b => b.ConfigureDeliveryInfo());
        builder.Navigation(o => o.DeliveryInfo).IsRequired();

        // Navigation to child entities — explicit backing field access
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        // Computed/transient properties
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.DomainEvents);
    }
}
