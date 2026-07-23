namespace SweetFlowerShop.Domain.Exceptions;

public sealed class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string action, string currentStatus)
        : base($"Cannot {action} an order with status '{currentStatus}'.") { }
}
