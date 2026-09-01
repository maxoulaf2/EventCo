namespace EventCo.Application.Auth.VerifyMagicLink;

public sealed record VerifyMagicLinkResult(
    Guid UserId,
    string Email,
    string DisplayName,
    string SessionToken,
    DateTime SessionExpiresAt);
