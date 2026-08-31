using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventCreatorCannotBeDemotedException : DomainException
{
    public Guid EventId { get; }
    public Guid CreatorUserId { get; }

    public EventCreatorCannotBeDemotedException(Guid eventId, Guid creatorUserId)
        : base($"Le créateur de l'événement ne peut pas être rétrogradé (EventId: {eventId}, CreatorUserId: {creatorUserId}).")
    {
        EventId = eventId;
        CreatorUserId = creatorUserId;
    }
}
