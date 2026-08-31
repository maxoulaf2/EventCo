using EventCo.Domain.Common;

namespace EventCo.Domain.ValueObjects.Exceptions;

public sealed class EmailInvalidFormatException : DomainException
{
    public string Value { get; }

    public EmailInvalidFormatException(string value)
        : base($"L'email '{value}' n'est pas valide.")
    {
        Value = value;
    }
}
