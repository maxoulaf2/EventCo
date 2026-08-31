using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class TaskAssigneeNotParticipantException : DomainException
{
    public Guid EventId { get; }
    public Guid TaskId { get; }
    public Guid UserId { get; }

    public TaskAssigneeNotParticipantException(Guid eventId, Guid taskId, Guid userId)
        : base($"Impossible d'assigner la tâche à un utilisateur qui n'est pas participant (EventId: {eventId}, TaskId: {taskId}, UserId: {userId}).")
    {
        EventId = eventId;
        TaskId = taskId;
        UserId = userId;
    }
}
