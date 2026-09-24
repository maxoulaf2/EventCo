namespace EventCo.Application.Common.Interfaces;

public interface IItemRealtimeNotifier
{
    Task NotifyItemCreated(ItemRealtimeDto item, CancellationToken cancellationToken);

    Task NotifyItemAssigned(ItemRealtimeDto item, CancellationToken cancellationToken);

    Task NotifyItemUnassigned(ItemRealtimeDto item, CancellationToken cancellationToken);

    Task NotifyItemDeleted(ItemDeletedRealtimeDto item, CancellationToken cancellationToken);
}
