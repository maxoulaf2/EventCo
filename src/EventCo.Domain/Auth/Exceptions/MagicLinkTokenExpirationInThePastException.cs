using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class MagicLinkTokenExpirationInThePastException : DomainException
{
    public DateTime ExpiresAt { get; }

    public MagicLinkTokenExpirationInThePastException(DateTime expiresAt)
        : base($"La date d'expiration doit être dans le futur (ExpiresAt: {expiresAt:O}).")
    {
        ExpiresAt = expiresAt;
    }
}
