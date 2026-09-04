using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public interface IEventRepository
{
    Task AddAsync(Event @event, CancellationToken cancellationToken);

    Task UpdateAsync(Event @event, CancellationToken cancellationToken);

    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Event>> GetByParticipantUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
