namespace EventCo.Application.Events.InviteParticipant;

public sealed record InviteParticipantResult(
    Guid EventId,
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    DateTime InvitedAt,
    bool HasJoined);
