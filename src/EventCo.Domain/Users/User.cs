using EventCo.Domain.Common;
using EventCo.Domain.Users.DomainEvents;
using EventCo.Domain.Users.Exceptions;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Users;

public class User : Entity
{
    public Email Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

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

        var user = new User(Guid.NewGuid(), email, displayName.Trim(), now);
        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return user;
    }

    internal static User Reconstitute(Guid id, Email email, string displayName, string? avatarUrl, DateTime createdAt) =>
        new(id, email, displayName, createdAt) { AvatarUrl = avatarUrl };

    public void UpdateProfile(string displayName, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new UserDisplayNameEmptyException(Id);

        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl;
        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
    }
}
