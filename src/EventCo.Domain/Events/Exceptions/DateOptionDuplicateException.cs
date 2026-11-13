using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class DateOptionDuplicateException : DomainException
{
    public Guid EventId { get; }

    public DateOptionDuplicateException(Guid eventId)
        : base($"Cette date est déjà proposée dans le sondage (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
