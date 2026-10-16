using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ItemCategoryCorrectedDomainEvent(EventItem Item) : IDomainEvent;
