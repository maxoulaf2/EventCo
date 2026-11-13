using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventDateRequiredException : DomainException
{
    public Guid EventId { get; }

    public EventDateRequiredException(Guid eventId)
        : base($"La date de l'événement est obligatoire (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
