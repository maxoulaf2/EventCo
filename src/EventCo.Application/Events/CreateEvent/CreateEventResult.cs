namespace EventCo.Application.Events.CreateEvent;

public sealed record CreateEventResult(
    Guid EventId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location,
    string? ImageUrl,
    Guid CreatedByUserId,
    string Status,
    DateTime CreatedAt);
