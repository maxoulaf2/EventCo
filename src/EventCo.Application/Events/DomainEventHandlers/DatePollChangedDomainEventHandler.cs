using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class DatePollChangedDomainEventHandler(IDatePollRealtimeNotifier datePollRealtimeNotifier)
    : IDomainEventHandler<DateOptionAddedDomainEvent>,
      IDomainEventHandler<DateOptionRemovedDomainEvent>,
      IDomainEventHandler<DateVoteCastDomainEvent>,
      IDomainEventHandler<DateVoteWithdrawnDomainEvent>,
      IDomainEventHandler<EventDateChosenDomainEvent>
{
    public Task Handle(DateOptionAddedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        datePollRealtimeNotifier.NotifyDatePollChanged(domainEvent.EventId, cancellationToken);

    public Task Handle(DateOptionRemovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        datePollRealtimeNotifier.NotifyDatePollChanged(domainEvent.EventId, cancellationToken);

    public Task Handle(DateVoteCastDomainEvent domainEvent, CancellationToken cancellationToken) =>
        datePollRealtimeNotifier.NotifyDatePollChanged(domainEvent.EventId, cancellationToken);

    public Task Handle(DateVoteWithdrawnDomainEvent domainEvent, CancellationToken cancellationToken) =>
        datePollRealtimeNotifier.NotifyDatePollChanged(domainEvent.EventId, cancellationToken);

    public Task Handle(EventDateChosenDomainEvent domainEvent, CancellationToken cancellationToken) =>
        datePollRealtimeNotifier.NotifyDatePollChanged(domainEvent.EventId, cancellationToken);
}
