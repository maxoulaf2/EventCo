namespace EventCo.Api.Contracts.Events;

public sealed record CreateEventRequest(string Title, string? Description, DateTime EventDate, string? Location);
