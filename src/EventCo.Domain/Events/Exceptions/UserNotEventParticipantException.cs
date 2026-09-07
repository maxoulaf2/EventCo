using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class UserNotEventParticipantException : DomainException
{
    public Guid EventId { get; }
    public Guid ActingUserId { get; }

    public UserNotEventParticipantException(Guid eventId, Guid actingUserId)
        : base($"Seuls les participants de l'événement peuvent le consulter (EventId: {eventId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ActingUserId = actingUserId;
    }
}
