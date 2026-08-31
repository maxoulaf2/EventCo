using EventCo.Domain.Common;

namespace EventCo.Domain.ValueObjects.Exceptions;

public sealed class EmailEmptyException : DomainException
{
    public EmailEmptyException() : base("L'email ne peut pas être vide.")
    {
    }
}
