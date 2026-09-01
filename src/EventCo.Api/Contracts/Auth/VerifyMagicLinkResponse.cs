namespace EventCo.Api.Contracts.Auth;

public sealed record VerifyMagicLinkResponse(Guid UserId, string Email, string DisplayName);
