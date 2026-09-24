using EventCo.Domain.Auth.DomainEvents;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.Common;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Auth;

// Code de connexion à usage unique envoyé par email (historiquement un lien magique, d'où le nom).
// Le code ne comptant que 6 chiffres, il est bloqué au-delà de MaxFailedAttempts essais erronés
// pour empêcher de le deviner par force brute pendant sa durée de validité.
public class MagicLinkToken : Entity
{
    public const int MaxFailedAttempts = 5;

    public Email Email { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public string? EventInviteLinkToken { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsConsumed => ConsumedAt is not null;

    public bool IsLocked => FailedAttempts >= MaxFailedAttempts;

    private MagicLinkToken(Guid id, Email email, string tokenHash, DateTime expiresAt, string? eventInviteLinkToken, DateTime createdAt) : base(id)
    {
        Email = email;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        EventInviteLinkToken = eventInviteLinkToken;
        CreatedAt = createdAt;
    }

    public static MagicLinkToken Create(Email email, string tokenHash, DateTime expiresAt, DateTime now, string? eventInviteLinkToken = null)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new MagicLinkTokenHashEmptyException();

        if (expiresAt <= now)
            throw new MagicLinkTokenExpirationInThePastException(expiresAt);

        var token = new MagicLinkToken(Guid.NewGuid(), email, tokenHash, expiresAt, eventInviteLinkToken, now);
        token.AddDomainEvent(new MagicLinkTokenCreatedDomainEvent(token.Id));
        return token;
    }

    internal static MagicLinkToken Reconstitute(Guid id, Email email, string tokenHash, DateTime expiresAt, DateTime? consumedAt, int failedAttempts, string? eventInviteLinkToken, DateTime createdAt) =>
        new(id, email, tokenHash, expiresAt, eventInviteLinkToken, createdAt) { ConsumedAt = consumedAt, FailedAttempts = failedAttempts };

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public void Consume(DateTime now)
    {
        if (IsConsumed)
            throw new MagicLinkTokenAlreadyConsumedException(Id, ConsumedAt);

        if (IsExpired(now))
            throw new MagicLinkTokenExpiredException(Id, ExpiresAt, now);

        if (IsLocked)
            throw new MagicLinkTokenLockedException(Id);

        ConsumedAt = now;
        AddDomainEvent(new MagicLinkTokenConsumedDomainEvent(Id));
    }

    public void RegisterFailedAttempt()
    {
        if (IsConsumed)
            throw new MagicLinkTokenAlreadyConsumedException(Id, ConsumedAt);

        if (IsLocked)
            throw new MagicLinkTokenLockedException(Id);

        FailedAttempts++;
        AddDomainEvent(new MagicLinkTokenFailedAttemptRegisteredDomainEvent(Id));
    }
}
