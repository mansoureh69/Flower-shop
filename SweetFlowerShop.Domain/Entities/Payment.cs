using SweetFlowerShop.Domain.Common;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.Events;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Entities;

/// <summary>
/// Aggregate Root - Tracks financial transactions for an Order.
/// Separate from Order because payment has its own lifecycle (retries, refunds, provider integration).
/// Dependent entities: PaymentTransaction
/// </summary>
public class Payment : AggregateRoot
{
    private readonly List<PaymentTransaction> _transactions = new();

    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions;

    public decimal TotalRefunded => _transactions
        .Where(t => t.Type == TransactionType.Refund)
        .Sum(t => t.Amount.Amount);

    private Payment() { }

    public Payment(Guid orderId, Money amount, PaymentMethod method)
    {
        if (amount.Amount <= 0)
            throw new InvalidOperationException("Payment amount must be greater than zero.");

        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(string? providerTransactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as paid.");

        Status = PaymentStatus.Paid;
        _transactions.Add(new PaymentTransaction(Id, Amount, TransactionType.Charge, providerTransactionId));
        RaiseDomainEvent(new PaymentCompletedEvent(Id, OrderId, Amount.Amount));
    }

    public void MarkAsFailed()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as failed.");

        Status = PaymentStatus.Failed;
        RaiseDomainEvent(new PaymentFailedEvent(Id, OrderId));
    }

    public void Refund(Money refundAmount, string? providerTransactionId = null)
    {
        if (Status != PaymentStatus.Paid)
            throw new InvalidOperationException("Only paid payments can be refunded.");

        if (refundAmount.Amount + TotalRefunded > Amount.Amount)
            throw new InvalidOperationException("Refund amount exceeds paid amount.");

        _transactions.Add(new PaymentTransaction(Id, refundAmount, TransactionType.Refund, providerTransactionId));

        if (TotalRefunded >= Amount.Amount)
            Status = PaymentStatus.Refunded;
    }
}
