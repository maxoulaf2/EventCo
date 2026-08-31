using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class UserNotEventCreatorException : DomainException
{
    public Guid EventId { get; }
    public Guid ActingUserId { get; }

    public UserNotEventCreatorException(Guid eventId, Guid actingUserId)
        : base($"Seul le créateur de l'événement peut effectuer cette action (EventId: {eventId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ActingUserId = actingUserId;
    }
}
