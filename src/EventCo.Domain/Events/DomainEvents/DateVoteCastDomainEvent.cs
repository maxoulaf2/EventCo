using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record DateVoteCastDomainEvent(Guid EventId, Guid DateOptionId, Guid UserId) : IDomainEvent;
