using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantNotFoundException : DomainException
{
    public Guid EventId { get; }
    public Guid UserId { get; }

    public ParticipantNotFoundException(Guid eventId, Guid userId)
        : base($"Cet utilisateur ne participe pas à l'événement (EventId: {eventId}, UserId: {userId}).")
    {
        EventId = eventId;
        UserId = userId;
    }
}
