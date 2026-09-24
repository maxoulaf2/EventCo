using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.AssignItem;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class AssignItemCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<AssignItemCommand>
{
    public async Task Handle(AssignItemCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.AssignItem(currentUserService.UserId!.Value, command.ItemId, command.UserId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
