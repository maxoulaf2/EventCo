namespace EventCo.Application.Events.GetAllEvents;

public sealed record GetAllEventsResult(IReadOnlyList<EventOverview> Events);

public sealed record EventOverview(
    Guid EventId,
    string Title,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    int ParticipantCount);
