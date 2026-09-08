using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public sealed record TaskRealtimeDto(
    Guid TaskId,
    Guid EventId,
    string Title,
    string Category,
    string? Quantity,
    Guid? AssignedToUserId,
    bool IsDone,
    DateTime CreatedAt)
{
    public static TaskRealtimeDto FromTask(EventTask task) => new(
        task.Id,
        task.EventId,
        task.Title,
        task.Category.ToString(),
        task.Quantity,
        task.AssignedToUserId,
        task.IsDone,
        task.CreatedAt);
}
