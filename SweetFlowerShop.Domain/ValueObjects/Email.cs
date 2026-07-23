using System.Text.RegularExpressions;
using SweetFlowerShop.Domain.Exceptions;

namespace SweetFlowerShop.Domain.ValueObjects;

public partial record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !EmailRegex().IsMatch(value))
            throw new InvalidEmailException(value ?? string.Empty);

        Value = value.Trim().ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public override string ToString() => Value;
}
