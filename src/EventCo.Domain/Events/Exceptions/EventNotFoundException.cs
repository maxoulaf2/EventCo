using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventNotFoundException : DomainException
{
    public Guid EventId { get; }

    public EventNotFoundException(Guid eventId)
        : base($"Événement introuvable (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
