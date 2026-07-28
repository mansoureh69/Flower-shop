using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PaymentId).IsRequired();
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(t => t.TransactionDate).IsRequired();
        builder.Property(t => t.ProviderTransactionId).HasMaxLength(200);

        // Value Object: Money
        builder.OwnsOne(t => t.Amount, b => b.ConfigureMoney("Amount", "Currency"));

        // Index for provider reconciliation queries
        builder.HasIndex(t => t.ProviderTransactionId)
            .HasFilter("\"ProviderTransactionId\" IS NOT NULL")
            .HasDatabaseName("IX_PaymentTransactions_ProviderId");
    }
}
