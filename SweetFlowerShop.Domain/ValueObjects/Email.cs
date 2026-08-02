using System.Text.RegularExpressions;
using SweetFlowerShop.Domain.Exceptions;

namespace SweetFlowerShop.Domain.ValueObjects;

public partial record Email
{
    public string Value { get; }

    public Email(string value)
    {
        var normalizedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedValue) || !EmailRegex().IsMatch(normalizedValue))
            throw new InvalidEmailException(value ?? string.Empty);

        Value = normalizedValue.ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public override string ToString() => Value;
}
