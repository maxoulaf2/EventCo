using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotDeleteOthersTaskException : DomainException
{
    public Guid EventId { get; }
    public Guid TaskId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotDeleteOthersTaskException(Guid eventId, Guid taskId, Guid actingUserId)
        : base($"Un simple participant ne peut supprimer que les tâches qu'il a créées (EventId: {eventId}, TaskId: {taskId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        TaskId = taskId;
        ActingUserId = actingUserId;
    }
}
