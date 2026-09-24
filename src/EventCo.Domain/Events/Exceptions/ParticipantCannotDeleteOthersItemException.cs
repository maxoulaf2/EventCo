using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotDeleteOthersItemException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotDeleteOthersItemException(Guid eventId, Guid itemId, Guid actingUserId)
        : base($"Un simple participant ne peut supprimer que les articles qu'il a créés (EventId: {eventId}, ItemId: {itemId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        ActingUserId = actingUserId;
    }
}
