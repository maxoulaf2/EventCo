using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.DomainEvents;

public sealed record MagicLinkTokenConsumedDomainEvent(Guid TokenId) : IDomainEvent;
