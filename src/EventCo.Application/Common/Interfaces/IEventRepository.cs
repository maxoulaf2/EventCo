using EventCo.Domain.Events;

namespace EventCo.Application.Common.Interfaces;

public interface IEventRepository: IRepository<Event>
{
    Task DeleteAsync(Event @event, CancellationToken cancellationToken);

    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Event?> GetByInviteLinkTokenAsync(string token, CancellationToken cancellationToken);

    Task<IReadOnlyList<Event>> GetByParticipantUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
