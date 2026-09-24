namespace EventCo.Api.Contracts.Events;

public sealed record EventItemResponse(
    Guid Id,
    Guid EventId,
    string Title,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt);
