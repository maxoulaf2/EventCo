using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;

namespace EventCo.Application.Common.Interfaces;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);  
}
