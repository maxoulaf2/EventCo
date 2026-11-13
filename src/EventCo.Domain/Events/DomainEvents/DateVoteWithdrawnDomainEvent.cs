using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record DateVoteWithdrawnDomainEvent(Guid EventId, Guid DateOptionId, Guid UserId) : IDomainEvent;
