using EventCo.Domain.Auth;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Tests.Auth;

public class MagicLinkTokenTests
{
    private static MagicLinkToken CreateToken(DateTime? expiresAt = null) =>
        MagicLinkToken.Create(Email.From("test@example.com"), "hashed-token", expiresAt ?? DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow);

    [Fact]
    public void Create_ExpiresAtInThePast_ThrowsMagicLinkTokenExpirationInThePastException()
    {
        Assert.Throws<MagicLinkTokenExpirationInThePastException>(() => CreateToken(DateTime.UtcNow.AddMinutes(-1)));
    }

    [Fact]
    public void Create_EmptyTokenHash_ThrowsMagicLinkTokenHashEmptyException()
    {
        Assert.Throws<MagicLinkTokenHashEmptyException>(
            () => MagicLinkToken.Create(Email.From("test@example.com"), " ", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow));
    }

    [Fact]
    public void IsExpired_NowEqualsExpiresAt_ReturnsTrue()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var token = CreateToken(expiresAt);

        Assert.True(token.IsExpired(expiresAt));
    }

    [Fact]
    public void Consume_NotExpiredAndNotConsumed_SetsConsumedAt()
    {
        var token = CreateToken();
        var now = DateTime.UtcNow;

        token.Consume(now);

        Assert.True(token.IsConsumed);
        Assert.Equal(now, token.ConsumedAt);
    }

    [Fact]
    public void Consume_AlreadyConsumed_ThrowsMagicLinkTokenAlreadyConsumedException()
    {
        var token = CreateToken();
        token.Consume(DateTime.UtcNow);

        Assert.Throws<MagicLinkTokenAlreadyConsumedException>(() => token.Consume(DateTime.UtcNow));
    }

    [Fact]
    public void Consume_TokenExpired_ThrowsMagicLinkTokenExpiredException()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(1));

        Assert.Throws<MagicLinkTokenExpiredException>(() => token.Consume(DateTime.UtcNow.AddMinutes(2)));
    }

    [Fact]
    public void Consume_TokenLocked_ThrowsMagicLinkTokenLockedException()
    {
        var token = CreateLockedToken();

        Assert.Throws<MagicLinkTokenLockedException>(() => token.Consume(DateTime.UtcNow));
    }

    [Fact]
    public void RegisterFailedAttempt_BelowMax_IncrementsFailedAttempts()
    {
        var token = CreateToken();

        token.RegisterFailedAttempt();

        Assert.Equal(1, token.FailedAttempts);
        Assert.False(token.IsLocked);
    }

    [Fact]
    public void RegisterFailedAttempt_MaxReached_LocksToken()
    {
        var token = CreateLockedToken();

        Assert.True(token.IsLocked);
    }

    [Fact]
    public void RegisterFailedAttempt_AlreadyLocked_ThrowsMagicLinkTokenLockedException()
    {
        var token = CreateLockedToken();

        Assert.Throws<MagicLinkTokenLockedException>(() => token.RegisterFailedAttempt());
    }

    [Fact]
    public void RegisterFailedAttempt_AlreadyConsumed_ThrowsMagicLinkTokenAlreadyConsumedException()
    {
        var token = CreateToken();
        token.Consume(DateTime.UtcNow);

        Assert.Throws<MagicLinkTokenAlreadyConsumedException>(() => token.RegisterFailedAttempt());
    }

    private static MagicLinkToken CreateLockedToken()
    {
        var token = CreateToken();
        for (var i = 0; i < MagicLinkToken.MaxFailedAttempts; i++)
            token.RegisterFailedAttempt();
        return token;
    }
}
