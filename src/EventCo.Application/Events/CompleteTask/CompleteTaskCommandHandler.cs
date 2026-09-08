using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.CompleteTask;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class CompleteTaskCommandHandler(ICurrentUserService currentUserService, IEventRepository eventRepository)
    : ICommandHandler<CompleteTaskCommand>
{
    public async Task Handle(CompleteTaskCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.CompleteTask(currentUserService.UserId!.Value, command.TaskId);

        await eventRepository.UpdateAsync(@event, cancellationToken);
    }
}
