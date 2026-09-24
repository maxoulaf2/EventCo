using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class LoginCodeExpirationInThePastException : DomainException
{
    public DateTime ExpiresAt { get; }

    public LoginCodeExpirationInThePastException(DateTime expiresAt)
        : base($"La date d'expiration doit être dans le futur (ExpiresAt: {expiresAt:O}).")
    {
        ExpiresAt = expiresAt;
    }
}
