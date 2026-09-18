using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.UpdateEvent;

public sealed class UpdateEventCommandHandler(ICurrentUserService currentUserService, IEventRepository eventRepository)
    : ICommandHandler<UpdateEventCommand, UpdateEventResult>
{
    public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        @event.UpdateDetails(currentUserService.UserId!.Value, request.Title, request.Description, request.EventDate, request.Location);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new UpdateEventResult(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.EventDate,
            @event.Location,
            @event.ImageUrl,
            @event.CreatedByUserId,
            @event.Status.ToString(),
            @event.CreatedAt);
    }
}
