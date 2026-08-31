using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventTaskTitleEmptyException : DomainException
{
    public Guid EventId { get; }

    public EventTaskTitleEmptyException(Guid eventId)
        : base($"Le titre de la tâche ne peut pas être vide (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
