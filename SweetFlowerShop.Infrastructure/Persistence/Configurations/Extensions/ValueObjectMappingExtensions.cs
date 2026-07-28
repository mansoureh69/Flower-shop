using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations.Extensions;

/// <summary>
/// Reusable Fluent API extensions for mapping Value Objects.
/// 
/// WHY THIS EXISTS:
/// Value Objects are mapped identically across multiple entities (e.g., Money appears in
/// Product, Payment, PaymentTransaction). Without a central place, changes to the VO
/// schema must be replicated in every configuration file — violating DRY and introducing
/// drift risk.
///
/// WHY GENERIC METHODS:
/// EF Core's OwnedNavigationBuilder requires concrete TOwner type. We use generic
/// constraints to work with any owner entity.
///
/// USAGE:
///   builder.OwnsOne(p => p.Price, b => b.ConfigureMoney("Price"));
///   builder.OwnsOne(p => p.Amount, b => b.ConfigureMoney("Amount"));
/// </summary>
public static class ValueObjectMappingExtensions
{
    /// <summary>
    /// Configures the Money value object columns.
    /// Stores Amount and Currency as inline columns in the owner's table.
    /// </summary>
    public static void ConfigureMoney<TOwner>(
        this OwnedNavigationBuilder<TOwner, Money> builder,
        string amountColumnName,
        string? currencyColumnName = null) where TOwner : class
    {
        builder.Property(x => x.Amount)
            .HasColumnName(amountColumnName)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasColumnName(currencyColumnName ?? $"{amountColumnName}_Currency")
            .HasMaxLength(3)
            .IsRequired();
    }

    /// <summary>
    /// Configures the Address value object columns.
    /// All properties stored with a configurable prefix to support multiple addresses
    /// on the same entity (e.g., "Billing_", "Shipping_").
    /// </summary>
    public static void ConfigureAddress<TOwner>(
        this OwnedNavigationBuilder<TOwner, Address> builder,
        string columnPrefix = "Address_") where TOwner : class
    {
        builder.Property(x => x.Street)
            .HasColumnName($"{columnPrefix}Street")
            .HasMaxLength(200);

        builder.Property(x => x.City)
            .HasColumnName($"{columnPrefix}City")
            .HasMaxLength(100);

        builder.Property(x => x.ZipCode)
            .HasColumnName($"{columnPrefix}ZipCode")
            .HasMaxLength(20);

        builder.Property(x => x.Country)
            .HasColumnName($"{columnPrefix}Country")
            .HasMaxLength(100);
    }

    /// <summary>
    /// Configures the Email value object as a single column with a unique index.
    /// </summary>
    public static void ConfigureEmail<TOwner>(
        this OwnedNavigationBuilder<TOwner, Email> builder,
        string columnName = "Email",
        bool isUnique = true) where TOwner : class
    {
        builder.Property(x => x.Value)
            .HasColumnName(columnName)
            .HasMaxLength(256)
            .IsRequired();

        if (isUnique)
        {
            builder.HasIndex(x => x.Value).IsUnique();
        }
    }

    /// <summary>
    /// Configures the DeliveryInfo value object columns with "Delivery_" prefix.
    /// </summary>
    public static void ConfigureDeliveryInfo<TOwner>(
        this OwnedNavigationBuilder<TOwner, DeliveryInfo> builder,
        string columnPrefix = "Delivery_") where TOwner : class
    {
        builder.Property(x => x.RecipientName)
            .HasColumnName($"{columnPrefix}RecipientName")
            .HasMaxLength(200);

        builder.Property(x => x.RecipientPhone)
            .HasColumnName($"{columnPrefix}RecipientPhone")
            .HasMaxLength(20);

        builder.Property(x => x.Street)
            .HasColumnName($"{columnPrefix}Street")
            .HasMaxLength(200);

        builder.Property(x => x.City)
            .HasColumnName($"{columnPrefix}City")
            .HasMaxLength(100);

        builder.Property(x => x.ZipCode)
            .HasColumnName($"{columnPrefix}ZipCode")
            .HasMaxLength(20);

        builder.Property(x => x.ScheduledDate)
            .HasColumnName($"{columnPrefix}ScheduledDate");

        builder.Property(x => x.GiftMessage)
            .HasColumnName($"{columnPrefix}GiftMessage")
            .HasMaxLength(500);
    }
}

