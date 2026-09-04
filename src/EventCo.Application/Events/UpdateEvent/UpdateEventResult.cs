namespace EventCo.Application.Events.UpdateEvent;

public sealed record UpdateEventResult(
    Guid EventId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    DateTime CreatedAt);
