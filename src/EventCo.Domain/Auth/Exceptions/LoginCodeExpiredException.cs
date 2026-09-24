using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class LoginCodeExpiredException : DomainException
{
    public Guid LoginCodeId { get; }
    public DateTime ExpiresAt { get; }
    public DateTime AttemptedAt { get; }

    public LoginCodeExpiredException(Guid loginCodeId, DateTime expiresAt, DateTime attemptedAt)
        : base($"Ce code de connexion a expiré (LoginCodeId: {loginCodeId}, ExpiresAt: {expiresAt:O}, AttemptedAt: {attemptedAt:O}).")
    {
        LoginCodeId = loginCodeId;
        ExpiresAt = expiresAt;
        AttemptedAt = attemptedAt;
    }
}
