using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventTitleEmptyException : DomainException
{
    public Guid? EventId { get; }

    public EventTitleEmptyException(Guid? eventId = null)
        : base("Le titre de l'événement ne peut pas être vide." + (eventId is not null ? $" (EventId: {eventId})" : string.Empty))
    {
        EventId = eventId;
    }
}
