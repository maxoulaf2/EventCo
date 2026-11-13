namespace EventCo.Application.Events.GetEventById;

public sealed record GetEventByIdResult(
    Guid EventId,
    string Title,
    string? Description,
    DateTime? EventDate,
    string? Location,
    string? ImageUrl,
    Guid CreatedByUserId,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<EventParticipantSummary> Participants,
    string? InviteLinkToken,
    IReadOnlyList<EventDateOptionSummary> DateOptions);

public sealed record EventDateOptionSummary(Guid DateOptionId, DateTime Date, IReadOnlyList<Guid> VoterUserIds);

public sealed record EventParticipantSummary(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    DateTime InvitedAt,
    string ParticipationStatus,
    string? AvatarUrl);
