using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public sealed record TaskRealtimeDto(
    Guid TaskId,
    Guid EventId,
    string Title,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt)
{
    public static TaskRealtimeDto FromTask(EventTask task) => new(
        task.Id,
        task.EventId,
        task.Title,
        task.Quantity,
        task.AssignedToUserId,
        task.CreatedAt);
}
