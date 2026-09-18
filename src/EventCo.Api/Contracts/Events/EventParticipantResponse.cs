namespace EventCo.Api.Contracts.Events;

public sealed record EventParticipantResponse(
    Guid EventId,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    DateTime InvitedAt,
    bool HasJoined,
    string ParticipationStatus);
