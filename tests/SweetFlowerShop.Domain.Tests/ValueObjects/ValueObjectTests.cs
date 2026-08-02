using SweetFlowerShop.Domain.Exceptions;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Domain.Tests.ValueObjects;

public sealed class ValueObjectTests
{
    [Fact]
    public void Money_NormalizesCurrencyAndUsesValueEquality()
    {
        var first = new Money(12.50m, "usd");
        var second = new Money(12.50m, "USD");

        Assert.Equal("USD", first.Currency);
        Assert.Equal(first, second);
    }

    [Fact]
    public void Money_RejectsNegativeAmount()
    {
        Assert.Throws<ArgumentException>(() => new Money(-0.01m, "USD"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Money_RejectsMissingCurrency(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, currency));
    }

    [Fact]
    public void Money_AddsValuesWithTheSameCurrency()
    {
        var result = new Money(10m, "USD").Add(new Money(2.50m, "USD"));

        Assert.Equal(new Money(12.50m, "USD"), result);
    }

    [Fact]
    public void Money_RejectsAdditionAcrossCurrencies()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new Money(10m, "USD").Add(new Money(10m, "EUR")));
    }

    [Fact]
    public void Email_NormalizesCaseAndWhitespace()
    {
        var email = new Email("  Customer@Example.COM ");

        Assert.Equal("customer@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("missing-domain@")]
    public void Email_RejectsInvalidAddresses(string value)
    {
        Assert.Throws<InvalidEmailException>(() => new Email(value));
    }

    [Fact]
    public void Address_UsesValueEquality()
    {
        var first = new Address("12 Garden Street", "Tehran", "12345", "Iran");
        var second = new Address("12 Garden Street", "Tehran", "12345", "Iran");

        Assert.Equal(first, second);
    }

    [Fact]
    public void DeliveryInfo_RejectsMissingRequiredFields()
    {
        Assert.Throws<ArgumentException>(() =>
            new DeliveryInfo("", "+1-555-0100", "12 Garden Street", "Tehran", "12345"));
        Assert.Throws<ArgumentException>(() =>
            new DeliveryInfo("Jane Doe", "", "12 Garden Street", "Tehran", "12345"));
        Assert.Throws<ArgumentException>(() =>
            new DeliveryInfo("Jane Doe", "+1-555-0100", "", "Tehran", "12345"));
        Assert.Throws<ArgumentException>(() =>
            new DeliveryInfo("Jane Doe", "+1-555-0100", "12 Garden Street", "", "12345"));
        Assert.Throws<ArgumentException>(() =>
            new DeliveryInfo("Jane Doe", "+1-555-0100", "12 Garden Street", "Tehran", ""));
    }
}
