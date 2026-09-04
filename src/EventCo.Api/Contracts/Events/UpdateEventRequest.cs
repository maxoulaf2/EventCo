namespace EventCo.Api.Contracts.Events;

public sealed record UpdateEventRequest(string Title, string? Description, DateTime EventDate, string? Location);
