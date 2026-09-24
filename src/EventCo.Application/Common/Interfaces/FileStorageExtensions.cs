using EventCo.Domain.Users;

namespace EventCo.Application.Common.Interfaces;

public static class FileStorageExtensions
{
    public static string? GetAvatarUrl(this IFileStorage fileStorage, User user) =>
        user.AvatarStorageKey is null ? null : fileStorage.GetPublicUrl(user.AvatarStorageKey);
}
