namespace SweetFlowerShop.Domain.ValueObjects;

/// <summary>
/// Value Object - Delivery details embedded in an Order.
/// No independent lifecycle or identity.
/// </summary>
public record DeliveryInfo
{
    public string RecipientName { get; }
    public string RecipientPhone { get; }
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }
    public DateTime? ScheduledDate { get; }
    public string? GiftMessage { get; }

    public DeliveryInfo(string recipientName, string recipientPhone,
        string street, string city, string zipCode,
        DateTime? scheduledDate = null, string? giftMessage = null)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Recipient name is required.", nameof(recipientName));

        if (string.IsNullOrWhiteSpace(recipientPhone))
            throw new ArgumentException("Recipient phone is required.", nameof(recipientPhone));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required.", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code is required.", nameof(zipCode));

        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        Street = street;
        City = city;
        ZipCode = zipCode;
        ScheduledDate = scheduledDate;
        GiftMessage = giftMessage;
    }
}
