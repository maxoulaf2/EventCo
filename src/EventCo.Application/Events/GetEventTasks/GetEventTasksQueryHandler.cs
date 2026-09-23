using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.GetEventTasks;

public sealed class GetEventTasksQueryHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IUserRepository userRepository) : ICommandHandler<GetEventTasksQuery, GetEventTasksResult>
{
    public async Task<GetEventTasksResult> Handle(GetEventTasksQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var currentUserId = currentUserService.UserId!.Value;
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        @event.EnsureCanBeViewedBy(currentUserId, currentUser?.IsAdmin == true);

        var tasks = @event.Tasks
            .OrderBy(t => t.CreatedAt)
            .Select(t => new EventTaskSummary(
                t.Id,
                t.EventId,
                t.Title,
                t.Category.ToString(),
                t.Quantity,
                t.AssignedToUserId,
                t.CreatedAt))
            .ToList();

        return new GetEventTasksResult(tasks);
    }
}
