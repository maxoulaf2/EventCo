namespace EventCo.Api.Contracts.Events;

public sealed record EventDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<EventParticipantResponse> Participants);
