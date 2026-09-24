using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotUnassignOthersItemException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotUnassignOthersItemException(Guid eventId, Guid itemId, Guid actingUserId)
        : base($"Un simple participant ne peut se désassigner que de ses propres articles (EventId: {eventId}, ItemId: {itemId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        ActingUserId = actingUserId;
    }
}
