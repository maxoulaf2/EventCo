using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ItemCreatedDomainEvent(EventItem Item) : IDomainEvent;
