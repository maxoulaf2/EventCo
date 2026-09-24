namespace EventCo.Api.Contracts.Auth;

public sealed record VerifyLoginCodeResponse(Guid UserId, string Email, string DisplayName, Guid? EventId);
