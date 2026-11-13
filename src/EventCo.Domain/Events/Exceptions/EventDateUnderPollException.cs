using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventDateUnderPollException : DomainException
{
    public Guid EventId { get; }

    public EventDateUnderPollException(Guid eventId)
        : base($"La date d'un événement en cours de sondage se fixe en retenant une option (EventId: {eventId}).")
    {
        EventId = eventId;
    }
}
