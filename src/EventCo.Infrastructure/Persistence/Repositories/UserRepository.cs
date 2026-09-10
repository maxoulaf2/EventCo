using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Users;
using EventCo.Domain.Users.DomainEvents;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(EventCoDbContext dbContext, DomainEventCollector domainEventCollector) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == email.Value, cancellationToken);
        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var entities = await dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync(cancellationToken);
        return entities.Select(UserMapper.ToDomain).ToList();
    }

    public async Task ApplyAsync(User user, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in user.DomainEvents)
        {
            switch (domainEvent)
            {
                case UserCreatedDomainEvent:
                    await InsertUser(user, cancellationToken);
                    break;
                case UserProfileUpdatedDomainEvent:
                    await UpdateUserProfile(user, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Domain event non géré par {nameof(UserRepository)} : {domainEvent.GetType().Name}.");
            }
        }

        domainEventCollector.Collect(user);
    }

    private async Task InsertUser(User user, CancellationToken cancellationToken)
    {
        var entity = UserMapper.ToEntity(user);
        await dbContext.Users.AddAsync(entity, cancellationToken);
    }

    private async Task UpdateUserProfile(User user, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users.FindAsync([user.Id], cancellationToken)
            ?? throw new InvalidOperationException($"UserEntity {user.Id} introuvable.");
        UserMapper.ApplyToEntity(user, entity);
    }
}
