namespace SweetFlowerShop.Domain.Exceptions;

public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException()
        : base("Price must be greater than zero.") { }
}
