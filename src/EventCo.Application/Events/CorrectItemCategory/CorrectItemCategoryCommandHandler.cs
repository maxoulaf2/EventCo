using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.CorrectItemCategory;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class CorrectItemCategoryCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository)
    : ICommandHandler<CorrectItemCategoryCommand>
{
    public async Task Handle(CorrectItemCategoryCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken)
            ?? throw new EventNotFoundException(command.EventId);

        var category = Enum.Parse<EventItemCategory>(command.Category);

        @event.CorrectItemCategory(currentUserService.UserId!.Value, command.ItemId, category);

        await eventRepository.ApplyAsync(@event, cancellationToken);
    }
}
