using System.Text.RegularExpressions;
using EventCo.Domain.ValueObjects.Exceptions;

namespace EventCo.Domain.ValueObjects;

public sealed partial class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new EmailEmptyException();

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw new EmailInvalidFormatException(value);

        return new Email(normalized);
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
