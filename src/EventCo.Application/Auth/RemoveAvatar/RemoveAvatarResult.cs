namespace EventCo.Application.Auth.RemoveAvatar;

public sealed record RemoveAvatarResult(Guid UserId, string Email, string DisplayName, bool IsAdmin, string? AvatarUrl);
