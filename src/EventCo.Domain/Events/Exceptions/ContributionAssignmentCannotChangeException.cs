using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ContributionAssignmentCannotChangeException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid ActingUserId { get; }

    public ContributionAssignmentCannotChangeException(Guid eventId, Guid itemId, Guid actingUserId)
        : base($"Un article apporté reste attribué à la personne qui l'apporte : il peut seulement être annulé par elle (EventId: {eventId}, ItemId: {itemId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        ActingUserId = actingUserId;
    }
}
