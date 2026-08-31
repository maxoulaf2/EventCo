using EventCo.Domain.Common;
using EventCo.Domain.Users.Exceptions;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Users;

public class User : Entity
{
    public Email Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    private User(Guid id, Email email, string displayName, DateTime createdAt) : base(id)
    {
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    public static User Create(Email email, string displayName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new UserDisplayNameEmptyException();

        return new User(Guid.NewGuid(), email, displayName.Trim(), now);
    }

    public void UpdateProfile(string displayName, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new UserDisplayNameEmptyException(Id);

        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl;
    }
}
