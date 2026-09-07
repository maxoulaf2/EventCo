using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(EventCoDbContext dbContext) : IUserRepository
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

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var entity = UserMapper.ToEntity(user);
        await dbContext.Users.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
