using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.TestDoubles;

public sealed class RecordingItemRealtimeNotifier : IItemRealtimeNotifier
{
    public List<ItemRealtimeDto> ItemCreatedNotifications { get; } = [];
    public List<ItemRealtimeDto> ItemAssignedNotifications { get; } = [];
    public List<ItemRealtimeDto> ItemUnassignedNotifications { get; } = [];
    public List<ItemDeletedRealtimeDto> ItemDeletedNotifications { get; } = [];

    public Task NotifyItemCreated(ItemRealtimeDto item, CancellationToken cancellationToken)
    {
        ItemCreatedNotifications.Add(item);
        return Task.CompletedTask;
    }

    public Task NotifyItemAssigned(ItemRealtimeDto item, CancellationToken cancellationToken)
    {
        ItemAssignedNotifications.Add(item);
        return Task.CompletedTask;
    }

    public Task NotifyItemUnassigned(ItemRealtimeDto item, CancellationToken cancellationToken)
    {
        ItemUnassignedNotifications.Add(item);
        return Task.CompletedTask;
    }

    public Task NotifyItemDeleted(ItemDeletedRealtimeDto item, CancellationToken cancellationToken)
    {
        ItemDeletedNotifications.Add(item);
        return Task.CompletedTask;
    }
}
