namespace EventCo.Api.Contracts.Events;

public sealed record CreateTaskRequest(string Title, string? Quantity);
