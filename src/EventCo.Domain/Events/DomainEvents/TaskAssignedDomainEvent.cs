using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record TaskAssignedDomainEvent(EventTask Task) : IDomainEvent;
