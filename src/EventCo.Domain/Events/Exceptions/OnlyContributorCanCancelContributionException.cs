using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class OnlyContributorCanCancelContributionException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid ActingUserId { get; }

    public OnlyContributorCanCancelContributionException(Guid eventId, Guid itemId, Guid actingUserId)
        : base($"Seule la personne qui apporte un article peut l'annuler, y compris vis-à-vis d'un organisateur (EventId: {eventId}, ItemId: {itemId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        ActingUserId = actingUserId;
    }
}
