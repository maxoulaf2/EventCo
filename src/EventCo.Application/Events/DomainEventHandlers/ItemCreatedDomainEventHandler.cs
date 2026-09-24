using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class ItemCreatedDomainEventHandler(IItemRealtimeNotifier itemRealtimeNotifier)
    : IDomainEventHandler<ItemCreatedDomainEvent>
{
    public Task Handle(ItemCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        itemRealtimeNotifier.NotifyItemCreated(ItemRealtimeDto.FromItem(domainEvent.Item), cancellationToken);
}
