namespace EventCo.Application.Events.CreateTask;

public sealed record CreateTaskResult(
    Guid TaskId,
    Guid EventId,
    string Title,
    string Category,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt);
