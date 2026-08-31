using EventCo.Domain.Common;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Auth;

public class MagicLinkToken : Entity
{
    public Email Email { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    public bool IsConsumed => ConsumedAt is not null;

    private MagicLinkToken()
    {
    }

    private MagicLinkToken(Guid id, Email email, string tokenHash, DateTime expiresAt) : base(id)
    {
        Email = email;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public static MagicLinkToken Create(Email email, string tokenHash, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Le hash du token ne peut pas être vide.");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("La date d'expiration doit être dans le futur.");

        return new MagicLinkToken(Guid.NewGuid(), email, tokenHash, expiresAt);
    }

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public void Consume(DateTime now)
    {
        if (IsConsumed)
            throw new DomainException("Ce lien de connexion a déjà été utilisé.");

        if (IsExpired(now))
            throw new DomainException("Ce lien de connexion a expiré.");

        ConsumedAt = now;
    }
}
