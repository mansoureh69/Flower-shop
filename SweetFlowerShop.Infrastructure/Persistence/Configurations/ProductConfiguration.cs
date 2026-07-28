using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        // Scalar properties
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.IsAvailable).HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).IsRequired();

        // Value Object: Money (owned type — stored as columns in Products table)
        builder.OwnsOne(p => p.Price, b => b.ConfigureMoney("Price", "Currency"));

        // Navigation to child entity — explicit backing field access
        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Indexes for query performance
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.IsAvailable)
            .HasFilter("\"IsAvailable\" = true")
            .HasDatabaseName("IX_Products_Available");

        // Soft delete
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.DeletedAtUtc);
        builder.HasIndex(p => p.IsDeleted)
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Products_NotDeleted");

        // Computed/transient properties — not persisted
        builder.Ignore(p => p.DomainEvents);
    }
}
