using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class DateOptionCountOutOfRangeException : DomainException
{
    public Guid EventId { get; }

    public DateOptionCountOutOfRangeException(Guid eventId)
        : base($"Un sondage de date compte entre 2 et 10 options (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
