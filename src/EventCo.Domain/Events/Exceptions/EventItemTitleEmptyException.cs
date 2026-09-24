using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventItemTitleEmptyException : DomainException
{
    public Guid EventId { get; }

    public EventItemTitleEmptyException(Guid eventId)
        : base($"Le titre de l'article ne peut pas être vide (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
