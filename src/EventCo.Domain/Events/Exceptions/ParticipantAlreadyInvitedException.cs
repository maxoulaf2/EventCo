using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantAlreadyInvitedException : DomainException
{
    public Guid EventId { get; }
    public Guid UserId { get; }

    public ParticipantAlreadyInvitedException(Guid eventId, Guid userId)
        : base($"Cet utilisateur est déjà invité à l'événement (EventId: {eventId}, UserId: {userId}).")
    {
        EventId = eventId;
        UserId = userId;
    }
}
