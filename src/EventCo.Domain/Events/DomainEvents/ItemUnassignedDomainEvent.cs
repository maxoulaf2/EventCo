using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ItemUnassignedDomainEvent(EventItem Item) : IDomainEvent;
