using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.DeleteItem;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class DeleteItemCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<DeleteItemCommand>
{
    public async Task Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.RemoveItem(currentUserService.UserId!.Value, command.ItemId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
