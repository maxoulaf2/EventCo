namespace EventCo.Application.Events.GetMyEvents;

public sealed record GetMyEventsResult(IReadOnlyList<MyEventSummary> Events);

public sealed record MyEventSummary(
    Guid EventId,
    string Title,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    string Role,
    bool HasJoined);
