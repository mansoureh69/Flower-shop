using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        // Scalar properties
        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(20);

        // Value Object: Email (required — a Customer always has an email)
        builder.OwnsOne(c => c.Email, b => b.ConfigureEmail());

        // Value Object: Address (optional — customer may not have set an address)
        builder.OwnsOne(c => c.DefaultAddress, b => b.ConfigureAddress());

        // Indexes for search
        builder.HasIndex(c => c.LastName)
            .HasDatabaseName("IX_Customers_LastName");

        // Soft delete
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Property(c => c.DeletedAtUtc);

        // Computed/transient properties
        builder.Ignore(c => c.FullName);
        builder.Ignore(c => c.DomainEvents);
    }
}
