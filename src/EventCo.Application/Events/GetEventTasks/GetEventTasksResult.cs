namespace EventCo.Application.Events.GetEventTasks;

public sealed record GetEventTasksResult(IReadOnlyList<EventTaskSummary> Tasks);

public sealed record EventTaskSummary(
    Guid TaskId,
    Guid EventId,
    string Title,
    string Category,
    string? Quantity,
    Guid? AssignedToUserId,
    bool IsDone,
    DateTime CreatedAt);
