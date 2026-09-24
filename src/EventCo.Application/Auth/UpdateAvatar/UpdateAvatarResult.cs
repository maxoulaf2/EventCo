namespace EventCo.Application.Auth.UpdateAvatar;

public sealed record UpdateAvatarResult(Guid UserId, string Email, string DisplayName, bool IsAdmin, string? AvatarUrl);
