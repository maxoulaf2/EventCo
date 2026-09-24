using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;
public sealed class ItemDeletedDomainEventHandler(IItemRealtimeNotifier itemRealtimeNotifier)
    : IDomainEventHandler<ItemDeletedDomainEvent>
{
    public Task Handle(ItemDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        itemRealtimeNotifier.NotifyItemDeleted(
            new ItemDeletedRealtimeDto(domainEvent.EventId, domainEvent.ItemId),
            cancellationToken);
}