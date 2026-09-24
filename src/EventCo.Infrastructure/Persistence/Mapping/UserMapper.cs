using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Entities;

namespace EventCo.Infrastructure.Persistence.Mapping;

internal static class UserMapper
{
    public static User ToDomain(UserEntity entity) =>
        User.Reconstitute(entity.Id, Email.From(entity.Email), entity.DisplayName, entity.AvatarStorageKey, entity.CreatedAt, entity.IsAdmin);

    public static UserEntity ToEntity(User domain) => new()
    {
        Id = domain.Id,
        Email = domain.Email.Value,
        DisplayName = domain.DisplayName,
        AvatarStorageKey = domain.AvatarStorageKey,
        CreatedAt = domain.CreatedAt,
        IsAdmin = domain.IsAdmin,
    };

    public static void ApplyToEntity(User domain, UserEntity entity)
    {
        entity.DisplayName = domain.DisplayName;
        entity.AvatarStorageKey = domain.AvatarStorageKey;
    }
}
