using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class MagicLinkTokenExpiredException : DomainException
{
    public Guid TokenId { get; }
    public DateTime ExpiresAt { get; }
    public DateTime AttemptedAt { get; }

    public MagicLinkTokenExpiredException(Guid tokenId, DateTime expiresAt, DateTime attemptedAt)
        : base($"Ce lien de connexion a expiré (TokenId: {tokenId}, ExpiresAt: {expiresAt:O}, AttemptedAt: {attemptedAt:O}).")
    {
        TokenId = tokenId;
        ExpiresAt = expiresAt;
        AttemptedAt = attemptedAt;
    }
}
