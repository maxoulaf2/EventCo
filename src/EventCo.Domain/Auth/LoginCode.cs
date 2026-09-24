using EventCo.Domain.Auth.DomainEvents;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.Common;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Auth;

// Code de connexion à usage unique envoyé par email.
// Le code ne comptant que 6 chiffres, il est bloqué au-delà de MaxFailedAttempts essais erronés
// pour empêcher de le deviner par force brute pendant sa durée de validité.
public class LoginCode : Entity
{
    public const int MaxFailedAttempts = 5;

    public Email Email { get; private set; } = null!;
    public string CodeHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public string? EventInviteLinkToken { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsConsumed => ConsumedAt is not null;

    public bool IsLocked => FailedAttempts >= MaxFailedAttempts;

    private LoginCode(Guid id, Email email, string codeHash, DateTime expiresAt, string? eventInviteLinkToken, DateTime createdAt) : base(id)
    {
        Email = email;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
        EventInviteLinkToken = eventInviteLinkToken;
        CreatedAt = createdAt;
    }

    public static LoginCode Create(Email email, string codeHash, DateTime expiresAt, DateTime now, string? eventInviteLinkToken = null)
    {
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new LoginCodeHashEmptyException();

        if (expiresAt <= now)
            throw new LoginCodeExpirationInThePastException(expiresAt);

        var loginCode = new LoginCode(Guid.NewGuid(), email, codeHash, expiresAt, eventInviteLinkToken, now);
        loginCode.AddDomainEvent(new LoginCodeCreatedDomainEvent(loginCode.Id));
        return loginCode;
    }

    internal static LoginCode Reconstitute(Guid id, Email email, string codeHash, DateTime expiresAt, DateTime? consumedAt, int failedAttempts, string? eventInviteLinkToken, DateTime createdAt) =>
        new(id, email, codeHash, expiresAt, eventInviteLinkToken, createdAt) { ConsumedAt = consumedAt, FailedAttempts = failedAttempts };

    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    public void Consume(DateTime now)
    {
        if (IsConsumed)
            throw new LoginCodeAlreadyConsumedException(Id, ConsumedAt);

        if (IsExpired(now))
            throw new LoginCodeExpiredException(Id, ExpiresAt, now);

        if (IsLocked)
            throw new LoginCodeLockedException(Id);

        ConsumedAt = now;
        AddDomainEvent(new LoginCodeConsumedDomainEvent(Id));
    }

    public void RegisterFailedAttempt()
    {
        if (IsConsumed)
            throw new LoginCodeAlreadyConsumedException(Id, ConsumedAt);

        if (IsLocked)
            throw new LoginCodeLockedException(Id);

        FailedAttempts++;
        AddDomainEvent(new LoginCodeFailedAttemptRegisteredDomainEvent(Id));
    }
}
