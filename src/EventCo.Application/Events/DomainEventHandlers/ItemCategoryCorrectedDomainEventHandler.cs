using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class ItemCategoryCorrectedDomainEventHandler(IItemRealtimeNotifier itemRealtimeNotifier)
    : IDomainEventHandler<ItemCategoryCorrectedDomainEvent>
{
    public Task Handle(ItemCategoryCorrectedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        itemRealtimeNotifier.NotifyItemCategoryCorrected(ItemRealtimeDto.FromItem(domainEvent.Item), cancellationToken);
}
