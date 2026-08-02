using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Dependent entity of Payment aggregate. Records individual charge/refund transactions.
/// </summary>
public class PaymentTransaction : Entity
{
    public Guid PaymentId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public string? ProviderTransactionId { get; private set; }

    private PaymentTransaction() { }

    internal PaymentTransaction(Guid paymentId, Money amount, TransactionType type, string? providerTransactionId = null)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("Payment ID is required.", nameof(paymentId));
        ArgumentNullException.ThrowIfNull(amount);
        if (amount.Amount <= 0)
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(amount));
        PaymentId = paymentId;
        Amount = amount;
        Type = type;
        TransactionDate = DateTime.UtcNow;
        ProviderTransactionId = providerTransactionId;
    }
}
