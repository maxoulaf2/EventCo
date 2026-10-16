using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotCorrectOthersItemCategoryException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotCorrectOthersItemCategoryException(Guid eventId, Guid itemId, Guid actingUserId)
        : base($"Un simple participant ne peut corriger la catégorie que des articles qu'il a créés (EventId: {eventId}, ItemId: {itemId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        ActingUserId = actingUserId;
    }
}
