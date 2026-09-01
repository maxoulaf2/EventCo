using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class MagicLinkTokenNotFoundException : DomainException
{
    public MagicLinkTokenNotFoundException() : base("Ce lien de connexion est invalide.")
    {
    }
}
