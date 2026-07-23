namespace SweetFlowerShop.Domain.Exceptions;

public sealed class InvalidCategoryLevelException : DomainException
{
    public const int MaxLevel = 3;

    public InvalidCategoryLevelException()
        : base($"Category nesting cannot exceed {MaxLevel} levels.") { }
}
