using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class DateOptionNotFoundException : DomainException
{
    public Guid EventId { get; }
    public Guid DateOptionId { get; }

    public DateOptionNotFoundException(Guid eventId, Guid dateOptionId)
        : base($"Option de date introuvable (EventId: {eventId}, DateOptionId: {dateOptionId}).")
    {
        EventId = eventId;
        DateOptionId = dateOptionId;
    }
}
