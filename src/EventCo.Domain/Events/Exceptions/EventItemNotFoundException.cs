using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventItemNotFoundException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }

    public EventItemNotFoundException(Guid eventId, Guid itemId)
        : base($"Article introuvable pour cet événement (EventId: {eventId}, ItemId: {itemId}).")
    {
        EventId = eventId;
        ItemId = itemId;
    }
}
