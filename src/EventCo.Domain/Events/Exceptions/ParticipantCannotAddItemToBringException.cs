using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotAddItemToBringException : DomainException
{
    public Guid EventId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotAddItemToBringException(Guid eventId, Guid actingUserId)
        : base($"Un simple participant ne peut ajouter que les articles qu'il apporte, pas d'article à prendre (EventId: {eventId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ActingUserId = actingUserId;
    }
}
