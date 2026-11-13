using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class DatePollNotOpenException : DomainException
{
    public Guid EventId { get; }

    public DatePollNotOpenException(Guid eventId)
        : base($"Cet événement n'a pas de sondage de date ouvert (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
