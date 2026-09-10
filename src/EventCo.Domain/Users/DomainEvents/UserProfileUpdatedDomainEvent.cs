using EventCo.Domain.Common;

namespace EventCo.Domain.Users.DomainEvents;

public sealed record UserProfileUpdatedDomainEvent(Guid UserId) : IDomainEvent;
