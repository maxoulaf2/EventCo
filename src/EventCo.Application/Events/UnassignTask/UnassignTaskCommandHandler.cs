using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.UnassignTask;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class UnassignTaskCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<UnassignTaskCommand>
{
    public async Task Handle(UnassignTaskCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.UnassignTask(currentUserService.UserId!.Value, command.TaskId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
