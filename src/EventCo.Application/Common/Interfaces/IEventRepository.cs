using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public interface IEventRepository
{
    Task AddAsync(Event @event, CancellationToken cancellationToken);

    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
