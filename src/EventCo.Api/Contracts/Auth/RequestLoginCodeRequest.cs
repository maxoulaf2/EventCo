namespace EventCo.Api.Contracts.Auth;

public sealed record RequestLoginCodeRequest(string Email, string? EventInviteLinkToken = null);
