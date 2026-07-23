namespace SweetFlowerShop.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email)
        : base($"'{email}' is not a valid email address.") { }
}
