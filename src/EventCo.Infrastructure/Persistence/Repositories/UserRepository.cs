using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(EventCoDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
