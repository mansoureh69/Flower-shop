using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.Events;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Tests.Entities;

public sealed class PaymentTests
{
    [Fact]
    public void NewPayment_StartsPendingWithoutTransactions()
    {
        var orderId = Guid.NewGuid();

        var payment = new Payment(orderId, new Money(100m, "USD"), PaymentMethod.CreditCard);

        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(PaymentMethod.CreditCard, payment.Method);
        Assert.Empty(payment.Transactions);
        Assert.Equal(0m, payment.TotalRefunded);
    }

    [Fact]
    public void NewPayment_RejectsZeroAmount()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new Payment(Guid.NewGuid(), new Money(0m, "USD"), PaymentMethod.CreditCard));
    }

    [Fact]
    public void MarkAsPaid_RecordsChargeAndRaisesCompletionEvent()
    {
        var payment = CreatePayment();

        payment.MarkAsPaid("provider-charge-1");

        Assert.Equal(PaymentStatus.Paid, payment.Status);
        var charge = Assert.Single(payment.Transactions);
        Assert.Equal(TransactionType.Charge, charge.Type);
        Assert.Equal(payment.Amount, charge.Amount);
        Assert.Equal("provider-charge-1", charge.ProviderTransactionId);

        var domainEvent = Assert.IsType<PaymentCompletedEvent>(Assert.Single(payment.DomainEvents));
        Assert.Equal(payment.Id, domainEvent.PaymentId);
        Assert.Equal(payment.OrderId, domainEvent.OrderId);
        Assert.Equal(payment.Amount.Amount, domainEvent.Amount);
    }

    [Fact]
    public void MarkAsPaid_RejectsDuplicateSuccess()
    {
        var payment = CreatePayment();
        payment.MarkAsPaid("provider-charge-1");

        Assert.Throws<InvalidOperationException>(() => payment.MarkAsPaid("provider-charge-2"));
        Assert.Single(payment.Transactions);
    }

    [Fact]
    public void MarkAsFailed_LeavesNoSuccessfulChargeAndRaisesFailureEvent()
    {
        var payment = CreatePayment();

        payment.MarkAsFailed();

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Empty(payment.Transactions);
        Assert.IsType<PaymentFailedEvent>(Assert.Single(payment.DomainEvents));
    }

    [Fact]
    public void Refund_RejectsPaymentThatIsNotPaid()
    {
        var payment = CreatePayment();

        Assert.Throws<InvalidOperationException>(() =>
            payment.Refund(new Money(10m, "USD"), "provider-refund-1"));
    }

    [Fact]
    public void PartialRefund_RecordsRefundAndKeepsPaidStatus()
    {
        var payment = CreatePaidPayment();

        payment.Refund(new Money(40m, "USD"), "provider-refund-1");

        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.Equal(40m, payment.TotalRefunded);
        Assert.Equal(2, payment.Transactions.Count);
    }

    [Fact]
    public void CumulativeFullRefund_MarksPaymentRefunded()
    {
        var payment = CreatePaidPayment();

        payment.Refund(new Money(40m, "USD"), "provider-refund-1");
        payment.Refund(new Money(60m, "USD"), "provider-refund-2");

        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(100m, payment.TotalRefunded);
    }

    [Fact]
    public void Refund_RejectsAmountAboveRemainingBalance()
    {
        var payment = CreatePaidPayment();
        payment.Refund(new Money(75m, "USD"), "provider-refund-1");

        Assert.Throws<InvalidOperationException>(() =>
            payment.Refund(new Money(26m, "USD"), "provider-refund-2"));
        Assert.Equal(75m, payment.TotalRefunded);
    }

    private static Payment CreatePayment() =>
        new(Guid.NewGuid(), new Money(100m, "USD"), PaymentMethod.CreditCard);

    private static Payment CreatePaidPayment()
    {
        var payment = CreatePayment();
        payment.MarkAsPaid("provider-charge-1");
        payment.ClearDomainEvents();
        return payment;
    }
}
