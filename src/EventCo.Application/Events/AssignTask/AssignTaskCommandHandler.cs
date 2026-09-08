using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.AssignTask;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class AssignTaskCommandHandler(ICurrentUserService currentUserService, IEventRepository eventRepository)
    : ICommandHandler<AssignTaskCommand>
{
    public async Task Handle(AssignTaskCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.AssignTask(currentUserService.UserId!.Value, command.TaskId, command.UserId);

        await eventRepository.UpdateAsync(@event, cancellationToken);
    }
}
