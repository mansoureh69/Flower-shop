namespace SweetFlowerShop.Domain.Exceptions;

public sealed class EmptyOrderException : DomainException
{
    public EmptyOrderException()
        : base("Order must contain at least one item.") { }
}
