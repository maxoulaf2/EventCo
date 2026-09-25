namespace EventCo.Application.Events.CreateItem;

public sealed record CreateItemResult(
    Guid ItemId,
    Guid EventId,
    string Title,
    string? Quantity,
    string Kind,
    Guid? AssignedToUserId,
    DateTime CreatedAt);
