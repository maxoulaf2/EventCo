using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ItemAssignedDomainEvent(EventItem Item) : IDomainEvent;
