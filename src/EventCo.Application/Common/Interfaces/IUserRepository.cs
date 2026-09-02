using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;

namespace EventCo.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}
