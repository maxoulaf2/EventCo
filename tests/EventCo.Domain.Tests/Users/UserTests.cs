using EventCo.Domain.Common;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_ValidData_SetsProperties()
    {
        var email = Email.Create("test@example.com");

        var user = User.Create(email, "Alice");

        Assert.Equal(email, user.Email);
        Assert.Equal("Alice", user.DisplayName);
        Assert.Null(user.AvatarUrl);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Create_EmptyDisplayName_ThrowsDomainException()
    {
        var email = Email.Create("test@example.com");

        Assert.Throws<DomainException>(() => User.Create(email, "  "));
    }

    [Fact]
    public void UpdateProfile_ValidData_UpdatesDisplayNameAndAvatar()
    {
        var user = User.Create(Email.Create("test@example.com"), "Alice");

        user.UpdateProfile("Alice B.", "https://example.com/avatar.png");

        Assert.Equal("Alice B.", user.DisplayName);
        Assert.Equal("https://example.com/avatar.png", user.AvatarUrl);
    }
}
