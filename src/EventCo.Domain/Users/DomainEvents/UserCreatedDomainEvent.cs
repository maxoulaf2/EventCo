using EventCo.Domain.Common;

namespace EventCo.Domain.Users.DomainEvents;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;
