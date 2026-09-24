using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ItemDeletedDomainEvent(Guid EventId, Guid ItemId) : IDomainEvent;
