using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.DomainEvents;

public sealed record MagicLinkTokenCreatedDomainEvent(Guid TokenId) : IDomainEvent;
