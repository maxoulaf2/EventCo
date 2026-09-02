namespace EventCo.Application.Common.Interfaces;

public sealed record SessionTokenData(Guid UserId, string Email, DateTime ExpiresAt);
