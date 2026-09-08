using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class ParticipantCannotToggleOthersTaskException : DomainException
{
    public Guid EventId { get; }
    public Guid TaskId { get; }
    public Guid ActingUserId { get; }

    public ParticipantCannotToggleOthersTaskException(Guid eventId, Guid taskId, Guid actingUserId)
        : base($"Un simple participant ne peut cocher/décocher que ses propres tâches (EventId: {eventId}, TaskId: {taskId}, ActingUserId: {actingUserId}).")
    {
        EventId = eventId;
        TaskId = taskId;
        ActingUserId = actingUserId;
    }
}
