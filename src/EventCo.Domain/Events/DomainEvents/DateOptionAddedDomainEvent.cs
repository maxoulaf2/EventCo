using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record DateOptionAddedDomainEvent(Guid EventId, Guid DateOptionId) : IDomainEvent;
