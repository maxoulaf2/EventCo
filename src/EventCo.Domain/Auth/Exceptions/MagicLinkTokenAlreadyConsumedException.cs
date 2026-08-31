using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class MagicLinkTokenAlreadyConsumedException : DomainException
{
    public Guid TokenId { get; }
    public DateTime? ConsumedAt { get; }

    public MagicLinkTokenAlreadyConsumedException(Guid tokenId, DateTime? consumedAt)
        : base($"Ce lien de connexion a déjà été utilisé (TokenId: {tokenId}, ConsumedAt: {consumedAt:O}).")
    {
        TokenId = tokenId;
        ConsumedAt = consumedAt;
    }
}
