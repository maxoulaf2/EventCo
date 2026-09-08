using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotAssignTaskToOthersException : DomainException
{
    public Guid EventId { get; }
    public Guid TaskId { get; }
    public Guid ActingUserId { get; }
    public Guid TargetUserId { get; }

    public ParticipantCannotAssignTaskToOthersException(Guid eventId, Guid taskId, Guid actingUserId, Guid targetUserId)
        : base($"Un simple participant ne peut s'assigner une tâche qu'à lui-même (EventId: {eventId}, TaskId: {taskId}, ActingUserId: {actingUserId}, TargetUserId: {targetUserId}).")
    {
        EventId = eventId;
        TaskId = taskId;
        ActingUserId = actingUserId;
        TargetUserId = targetUserId;
    }
}
