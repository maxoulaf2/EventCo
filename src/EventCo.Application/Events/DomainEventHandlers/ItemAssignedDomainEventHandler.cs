using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class ItemAssignedDomainEventHandler(IItemRealtimeNotifier itemRealtimeNotifier)
    : IDomainEventHandler<ItemAssignedDomainEvent>
{
    public Task Handle(ItemAssignedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        itemRealtimeNotifier.NotifyItemAssigned(ItemRealtimeDto.FromItem(domainEvent.Item), cancellationToken);
}


