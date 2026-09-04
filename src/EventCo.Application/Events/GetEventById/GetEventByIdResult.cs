namespace EventCo.Application.Events.GetEventById;

public sealed record GetEventByIdResult(
    Guid EventId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    DateTime CreatedAt);
