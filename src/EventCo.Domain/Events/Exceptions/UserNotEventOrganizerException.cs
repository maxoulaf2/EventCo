using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class UserNotEventOrganizerException : DomainException
{
    public Guid EventId { get; }
    public Guid ActingUserId { get; }

    public UserNotEventOrganizerException(Guid eventId, Guid actingUserId)
        : base($"Seuls le créateur ou les co-organisateurs de l'événement peuvent effectuer cette action (EventId: {eventId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ActingUserId = actingUserId;
    }
}
