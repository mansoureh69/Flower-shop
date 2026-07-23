namespace SweetFlowerShop.Domain.Exceptions;

public sealed class EmptyNameException : DomainException
{
    public EmptyNameException(string entityName)
        : base($"{entityName} name cannot be empty.") { }
}
