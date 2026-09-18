namespace EventCo.Application.Events.UpdateEvent;

public sealed record UpdateEventResult(
    Guid EventId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location,
    string? ImageUrl,
    Guid CreatedByUserId,
    string Status,
    DateTime CreatedAt);
