using EventCo.Domain.Common;

namespace EventCo.Domain.Events;

public class EventParticipant : Entity
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public ParticipantRole Role { get; private set; }
    public DateTime InvitedAt { get; private set; }
    public DateTime? JoinedAt { get; private set; }

    public bool HasJoined => JoinedAt is not null;

    internal EventParticipant(Guid eventId, Guid userId, ParticipantRole role, DateTime invitedAt)
        : base(Guid.NewGuid())
    {
        EventId = eventId;
        UserId = userId;
        Role = role;
        InvitedAt = invitedAt;
    }

    private EventParticipant(Guid id, Guid eventId, Guid userId, ParticipantRole role, DateTime invitedAt, DateTime? joinedAt)
        : base(id)
    {
        EventId = eventId;
        UserId = userId;
        Role = role;
        InvitedAt = invitedAt;
        JoinedAt = joinedAt;
    }

    internal static EventParticipant Reconstitute(Guid id, Guid eventId, Guid userId, ParticipantRole role, DateTime invitedAt, DateTime? joinedAt) =>
        new(id, eventId, userId, role, invitedAt, joinedAt);

    internal void Join(DateTime joinedAt) => JoinedAt = joinedAt;

    internal void ChangeRole(ParticipantRole role) => Role = role;
}
