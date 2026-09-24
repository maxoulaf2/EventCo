using EventCo.Domain.Users;
using EventCo.Domain.Users.DomainEvents;
using EventCo.Domain.Users.Exceptions;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_ValidData_SetsProperties()
    {
        var email = Email.From("test@example.com");

        var user = User.Create(email, "Alice", DateTime.UtcNow);

        Assert.Equal(email, user.Email);
        Assert.Equal("Alice", user.DisplayName);
        Assert.Null(user.AvatarStorageKey);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Create_ValidData_IsNotAdminByDefault()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);

        Assert.False(user.IsAdmin);
    }

    [Fact]
    public void Create_EmptyDisplayName_ThrowsUserDisplayNameEmptyException()
    {
        var email = Email.From("test@example.com");

        Assert.Throws<UserDisplayNameEmptyException>(() => User.Create(email, "  ", DateTime.UtcNow));
    }

    [Fact]
    public void UpdateProfile_ValidData_UpdatesDisplayName()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);

        user.UpdateProfile("Alice B.");

        Assert.Equal("Alice B.", user.DisplayName);
    }

    [Fact]
    public void UpdateProfile_EmptyDisplayName_ThrowsUserDisplayNameEmptyException()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);

        Assert.Throws<UserDisplayNameEmptyException>(() => user.UpdateProfile("  "));
    }

    [Fact]
    public void ChangeAvatar_ValidKey_SetsAvatarStorageKeyAndRaisesProfileUpdated()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);
        user.ClearDomainEvents();

        user.ChangeAvatar("avatars/alice/photo.jpg");

        Assert.Equal("avatars/alice/photo.jpg", user.AvatarStorageKey);
        Assert.IsType<UserProfileUpdatedDomainEvent>(Assert.Single(user.DomainEvents));
    }

    [Fact]
    public void ChangeAvatar_EmptyKey_ThrowsArgumentException()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => user.ChangeAvatar(" "));
    }

    [Fact]
    public void RemoveAvatar_WithAvatar_ClearsAvatarStorageKey()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);
        user.ChangeAvatar("avatars/alice/photo.jpg");

        user.RemoveAvatar();

        Assert.Null(user.AvatarStorageKey);
    }

    [Fact]
    public void RemoveAvatar_WithoutAvatar_RaisesNoDomainEvent()
    {
        var user = User.Create(Email.From("test@example.com"), "Alice", DateTime.UtcNow);
        user.ClearDomainEvents();

        user.RemoveAvatar();

        Assert.Empty(user.DomainEvents);
    }
}
