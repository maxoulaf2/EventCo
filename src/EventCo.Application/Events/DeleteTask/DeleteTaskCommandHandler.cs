using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.DeleteTask;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class DeleteTaskCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    ITaskRealtimeNotifier taskRealtimeNotifier)
    : ICommandHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.RemoveTask(currentUserService.UserId!.Value, command.TaskId);

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await taskRealtimeNotifier.NotifyTaskDeleted(new TaskDeletedRealtimeDto(command.EventId, command.TaskId), cancellationToken);
    }
}
