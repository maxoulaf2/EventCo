using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventCreatorCannotBeRemovedException : DomainException
{
    public Guid EventId { get; }
    public Guid CreatorUserId { get; }

    public EventCreatorCannotBeRemovedException(Guid eventId, Guid creatorUserId)
        : base($"Le créateur de l'événement ne peut pas être retiré (EventId: {eventId}, CreatorUserId: {creatorUserId}).")
    {
        EventId = eventId;
        CreatorUserId = creatorUserId;
    }
}
