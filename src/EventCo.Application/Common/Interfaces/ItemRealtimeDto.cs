using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public sealed record ItemRealtimeDto(
    Guid ItemId,
    Guid EventId,
    string Title,
    string? Quantity,
    string Kind,
    Guid? AssignedToUserId,
    DateTime CreatedAt)
{
    public static ItemRealtimeDto FromItem(EventItem item) => new(
        item.Id,
        item.EventId,
        item.Title,
        item.Quantity,
        item.Kind.ToString(),
        item.AssignedToUserId,
        item.CreatedAt);
}
