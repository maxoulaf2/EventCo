using EventCo.Domain.Auth;
using EventCo.Domain.Common;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Tests.Auth;

public class MagicLinkTokenTests
{
    private static MagicLinkToken CreateToken(DateTime? expiresAt = null) =>
        MagicLinkToken.Create(Email.Create("test@example.com"), "hashed-token", expiresAt ?? DateTime.UtcNow.AddMinutes(15));

    [Fact]
    public void Create_ExpiresAtInThePast_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => CreateToken(DateTime.UtcNow.AddMinutes(-1)));
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
    public void Consume_AlreadyConsumed_ThrowsDomainException()
    {
        var token = CreateToken();
        token.Consume(DateTime.UtcNow);

        Assert.Throws<DomainException>(() => token.Consume(DateTime.UtcNow));
    }

    [Fact]
    public void Consume_TokenExpired_ThrowsDomainException()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(1));

        Assert.Throws<DomainException>(() => token.Consume(DateTime.UtcNow.AddMinutes(2)));
    }
}
