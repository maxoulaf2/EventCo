using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ItemAssigneeNotParticipantException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid UserId { get; }

    public ItemAssigneeNotParticipantException(Guid eventId, Guid itemId, Guid userId)
        : base($"Impossible d'assigner l'article à un utilisateur qui n'est pas participant (EventId: {eventId}, ItemId: {itemId}, UserId: {userId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        UserId = userId;
    }
}
