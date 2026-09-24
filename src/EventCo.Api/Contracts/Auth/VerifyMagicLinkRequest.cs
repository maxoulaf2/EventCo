namespace EventCo.Api.Contracts.Auth;

public sealed record VerifyMagicLinkRequest(string Email, string Code);
