namespace EventCo.Api.Contracts.Events;

public sealed record EventTaskResponse(
    Guid Id,
    Guid EventId,
    string Title,
    string Category,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt);
