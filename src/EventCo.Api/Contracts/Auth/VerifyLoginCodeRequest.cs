namespace EventCo.Api.Contracts.Auth;

public sealed record VerifyLoginCodeRequest(string Email, string Code);
