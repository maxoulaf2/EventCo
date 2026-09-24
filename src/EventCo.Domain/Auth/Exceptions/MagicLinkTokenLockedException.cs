using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.Exceptions;

public sealed class MagicLinkTokenLockedException : DomainException
{
    public Guid TokenId { get; }

    public MagicLinkTokenLockedException(Guid tokenId)
        : base($"Ce code de connexion est bloqué après trop d'essais erronés (TokenId: {tokenId}).")
    {
        TokenId = tokenId;
    }
}
