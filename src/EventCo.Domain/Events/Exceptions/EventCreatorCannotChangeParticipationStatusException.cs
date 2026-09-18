using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventCreatorCannotChangeParticipationStatusException : DomainException
{
    public Guid EventId { get; }
    public Guid CreatorUserId { get; }

    public EventCreatorCannotChangeParticipationStatusException(Guid eventId, Guid creatorUserId)
        : base($"Le créateur de l'événement ne peut pas modifier son propre statut de participation (EventId: {eventId}, CreatorUserId: {creatorUserId}).")
    {
        EventId = eventId;
        CreatorUserId = creatorUserId;
    }
}
