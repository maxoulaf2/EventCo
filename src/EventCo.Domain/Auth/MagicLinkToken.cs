using EventCo.Domain.Auth.DomainEvents;
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

        var token = new MagicLinkToken(Guid.NewGuid(), email, tokenHash, expiresAt);
        token.AddDomainEvent(new MagicLinkTokenCreatedDomainEvent(token.Id));
        return token;
    }

    internal static MagicLinkToken Reconstitute(Guid id, Email email, string tokenHash, DateTime expiresAt, DateTime? consumedAt) =>
        new(id, email, tokenHash, expiresAt) { ConsumedAt = consumedAt };

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public void Consume(DateTime now)
    {
        if (IsConsumed)
            throw new MagicLinkTokenAlreadyConsumedException(Id, ConsumedAt);

        if (IsExpired(now))
            throw new MagicLinkTokenExpiredException(Id, ExpiresAt, now);

        ConsumedAt = now;
        AddDomainEvent(new MagicLinkTokenConsumedDomainEvent(Id));
    }
}
