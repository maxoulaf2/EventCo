namespace EventCo.Api.Contracts.Events;

public sealed record EventInvitePreviewResponse(
    Guid EventId,
    string Title,
    DateTime EventDate,
    string? Location,
    string CreatedByDisplayName);
