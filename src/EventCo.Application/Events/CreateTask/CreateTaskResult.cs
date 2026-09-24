namespace EventCo.Application.Events.CreateTask;

public sealed record CreateTaskResult(
    Guid TaskId,
    Guid EventId,
    string Title,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt);
