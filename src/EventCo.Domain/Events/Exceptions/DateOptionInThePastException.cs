using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class DateOptionInThePastException : DomainException
{
    public Guid EventId { get; }

    public DateOptionInThePastException(Guid eventId)
        : base($"Une option de date doit être dans le futur (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
