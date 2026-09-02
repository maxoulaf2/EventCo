namespace EventCo.Api.Contracts.Auth;

public sealed record CurrentUserResponse(Guid UserId, string Email, string DisplayName);
