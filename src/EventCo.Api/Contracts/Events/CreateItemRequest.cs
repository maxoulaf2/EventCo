namespace EventCo.Api.Contracts.Events;

public sealed record CreateItemRequest(string Title, string? Quantity, string Kind);
