using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class ItemUnassignedDomainEventHandler(IItemRealtimeNotifier itemRealtimeNotifier)
    : IDomainEventHandler<ItemUnassignedDomainEvent>
{
    public Task Handle(ItemUnassignedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        itemRealtimeNotifier.NotifyItemUnassigned(ItemRealtimeDto.FromItem(domainEvent.Item), cancellationToken);
}
