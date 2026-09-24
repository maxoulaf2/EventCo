namespace EventCo.Application.Events.GetEventItems;

public sealed record GetEventItemsResult(IReadOnlyList<EventItemSummary> Items);

public sealed record EventItemSummary(
    Guid ItemId,
    Guid EventId,
    string Title,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt);
