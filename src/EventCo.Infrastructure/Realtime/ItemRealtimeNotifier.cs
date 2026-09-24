using EventCo.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EventCo.Infrastructure.Realtime;

internal sealed class ItemRealtimeNotifier(IHubContext<EventHub> hubContext) : IItemRealtimeNotifier
{
    public Task NotifyItemCreated(ItemRealtimeDto item, CancellationToken cancellationToken) =>
        Send(item.EventId, "ItemCreated", item, cancellationToken);

    public Task NotifyItemAssigned(ItemRealtimeDto item, CancellationToken cancellationToken) =>
        Send(item.EventId, "ItemAssigned", item, cancellationToken);

    public Task NotifyItemUnassigned(ItemRealtimeDto item, CancellationToken cancellationToken) =>
        Send(item.EventId, "ItemUnassigned", item, cancellationToken);

    public Task NotifyItemDeleted(ItemDeletedRealtimeDto item, CancellationToken cancellationToken) =>
        Send(item.EventId, "ItemDeleted", item, cancellationToken);

    private Task Send(Guid eventId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(EventHub.GroupName(eventId)).SendAsync(method, payload, cancellationToken);
}
