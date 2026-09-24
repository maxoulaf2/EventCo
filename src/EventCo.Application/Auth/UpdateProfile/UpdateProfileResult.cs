namespace EventCo.Application.Auth.UpdateProfile;

public sealed record UpdateProfileResult(Guid UserId, string Email, string DisplayName, bool IsAdmin, string? AvatarUrl);
