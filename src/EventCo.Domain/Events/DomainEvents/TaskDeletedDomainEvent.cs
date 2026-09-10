using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record TaskDeletedDomainEvent(Guid EventId, Guid TaskId) : IDomainEvent;
