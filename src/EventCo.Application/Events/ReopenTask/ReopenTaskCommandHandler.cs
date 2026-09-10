using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.ReopenTask;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class ReopenTaskCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<ReopenTaskCommand>
{
    public async Task Handle(ReopenTaskCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.ReopenTask(currentUserService.UserId!.Value, command.TaskId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
