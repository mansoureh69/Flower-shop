using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        // Scalar properties
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Level).IsRequired();
        builder.Property(c => c.ParentCategoryId);

        builder.HasOne<Category>().WithMany().HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing hierarchy
        builder.HasIndex(c => c.ParentCategoryId);

        // Unique name within same parent (prevent duplicate category names at same level)
        builder.HasIndex(c => new { c.ParentCategoryId, c.Name })
            .IsUnique()
            .HasDatabaseName("IX_Categories_Parent_Name");

        // Soft delete
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Property(c => c.DeletedAtUtc);

        builder.Ignore(c => c.DomainEvents);
    }
}
