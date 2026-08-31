using EventCo.Domain.Users;
using EventCo.Domain.Users.Exceptions;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_ValidData_SetsProperties()
    {
        var email = Email.Create("test@example.com");

        var user = User.Create(email, "Alice", DateTime.UtcNow);

        Assert.Equal(email, user.Email);
        Assert.Equal("Alice", user.DisplayName);
        Assert.Null(user.AvatarUrl);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Create_EmptyDisplayName_ThrowsUserDisplayNameEmptyException()
    {
        var email = Email.Create("test@example.com");

        Assert.Throws<UserDisplayNameEmptyException>(() => User.Create(email, "  ", DateTime.UtcNow));
    }

    [Fact]
    public void UpdateProfile_ValidData_UpdatesDisplayNameAndAvatar()
    {
        var user = User.Create(Email.Create("test@example.com"), "Alice", DateTime.UtcNow);

        user.UpdateProfile("Alice B.", "https://example.com/avatar.png");

        Assert.Equal("Alice B.", user.DisplayName);
        Assert.Equal("https://example.com/avatar.png", user.AvatarUrl);
    }
}
