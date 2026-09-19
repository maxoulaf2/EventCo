namespace EventCo.Application.Events.GetEventInvitePreviewByToken;

public sealed record EventInvitePreviewResult(
    Guid EventId,
    string Title,
    DateTime EventDate,
    string? Location,
    string CreatedByDisplayName);
