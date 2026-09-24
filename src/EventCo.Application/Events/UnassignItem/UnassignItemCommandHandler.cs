using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.UnassignItem;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class UnassignItemCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<UnassignItemCommand>
{
    public async Task Handle(UnassignItemCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        @event.UnassignItem(currentUserService.UserId!.Value, command.ItemId);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
