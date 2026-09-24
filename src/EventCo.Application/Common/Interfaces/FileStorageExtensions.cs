using EventCo.Domain.Events;
using EventCo.Domain.Users;

namespace EventCo.Application.Common.Interfaces;

public static class FileStorageExtensions
{
    public static string? GetAvatarUrl(this IFileStorage fileStorage, User user) =>
        user.AvatarStorageKey is null ? null : fileStorage.GetPublicUrl(FileStorageBucket.Avatars, user.AvatarStorageKey);

    public static string? GetEventImageUrl(this IFileStorage fileStorage, Event @event) =>
        @event.ImageStorageKey is null ? null : fileStorage.GetPublicUrl(FileStorageBucket.EventImages, @event.ImageStorageKey);
}
