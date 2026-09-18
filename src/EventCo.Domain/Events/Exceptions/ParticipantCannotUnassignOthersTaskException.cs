using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotUnassignOthersTaskException : DomainException
{
    public Guid EventId { get; }
    public Guid TaskId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotUnassignOthersTaskException(Guid eventId, Guid taskId, Guid actingUserId)
        : base($"Un simple participant ne peut se désassigner que de ses propres tâches (EventId: {eventId}, TaskId: {taskId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        TaskId = taskId;
        ActingUserId = actingUserId;
    }
}
