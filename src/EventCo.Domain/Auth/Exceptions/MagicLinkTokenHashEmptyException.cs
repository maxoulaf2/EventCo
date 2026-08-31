using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class MagicLinkTokenHashEmptyException : DomainException
{
    public MagicLinkTokenHashEmptyException() : base("Le hash du token ne peut pas être vide.")
    {
    }
}
