using EventCo.Domain.Auth.Exceptions;
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

    public static MagicLinkToken Create(Email email, string tokenHash, DateTime expiresAt, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new MagicLinkTokenHashEmptyException();

        if (expiresAt <= now)
            throw new MagicLinkTokenExpirationInThePastException(expiresAt);

        return new MagicLinkToken(Guid.NewGuid(), email, tokenHash, expiresAt);
    }

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public void Consume(DateTime now)
    {
        if (IsConsumed)
            throw new MagicLinkTokenAlreadyConsumedException(Id, ConsumedAt);

        if (IsExpired(now))
            throw new MagicLinkTokenExpiredException(Id, ExpiresAt, now);

        ConsumedAt = now;
    }
}
