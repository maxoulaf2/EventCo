using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record TaskUnassignedDomainEvent(EventTask Task) : IDomainEvent;
