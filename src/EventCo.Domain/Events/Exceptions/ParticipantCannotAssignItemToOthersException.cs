using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotAssignItemToOthersException : DomainException
{
    public Guid EventId { get; }
    public Guid ItemId { get; }
    public Guid ActingUserId { get; }
    public Guid TargetUserId { get; }

    public ParticipantCannotAssignItemToOthersException(Guid eventId, Guid itemId, Guid actingUserId, Guid targetUserId)
        : base($"Un simple participant ne peut s'assigner un article qu'à lui-même (EventId: {eventId}, ItemId: {itemId}, ActingUserId: {actingUserId}, TargetUserId: {targetUserId}).")
    {
        EventId = eventId;
        ItemId = itemId;
        ActingUserId = actingUserId;
        TargetUserId = targetUserId;
    }
}
