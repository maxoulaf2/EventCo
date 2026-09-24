using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public sealed record ItemRealtimeDto(
    Guid ItemId,
    Guid EventId,
    string Title,
    string? Quantity,
    Guid? AssignedToUserId,
    DateTime CreatedAt)
{
    public static ItemRealtimeDto FromItem(EventItem item) => new(
        item.Id,
        item.EventId,
        item.Title,
        item.Quantity,
        item.AssignedToUserId,
        item.CreatedAt);
}
