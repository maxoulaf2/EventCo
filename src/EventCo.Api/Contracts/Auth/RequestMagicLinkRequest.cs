namespace EventCo.Api.Contracts.Auth;

public sealed record RequestMagicLinkRequest(string Email, string? EventInviteLinkToken = null);
