using EventCo.Domain.Common;

namespace EventCo.Domain.Events.Exceptions;

public sealed class EventTaskNotFoundException : DomainException
{
    public Guid EventId { get; }
    public Guid TaskId { get; }

    public EventTaskNotFoundException(Guid eventId, Guid taskId)
        : base($"Tâche introuvable pour cet événement (EventId: {eventId}, TaskId: {taskId}).")
    {
        EventId = eventId;
        TaskId = taskId;
    }
}
