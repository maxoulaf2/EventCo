using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.CreateItem;

public sealed class CreateItemCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateItemCommand, CreateItemResult>
{
    public async Task<CreateItemResult> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var now = dateTimeProvider.UtcNow;

        var item = @event.AddItem(currentUserService.UserId!.Value, request.Title, request.Quantity, now);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new CreateItemResult(
            item.Id,
            @event.Id,
            item.Title,
            item.Quantity,
            item.AssignedToUserId,
            item.CreatedAt);
    }
}
