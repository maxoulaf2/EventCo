using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record DateOptionRemovedDomainEvent(Guid EventId, Guid DateOptionId) : IDomainEvent;
