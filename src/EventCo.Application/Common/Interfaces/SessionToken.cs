namespace EventCo.Application.Common.Interfaces;

public sealed record SessionToken(string Value, DateTime ExpiresAt);
