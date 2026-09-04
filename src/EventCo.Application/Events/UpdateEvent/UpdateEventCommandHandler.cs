using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.UpdateEvent;

public sealed class UpdateEventCommandHandler(IEventRepository eventRepository)
    : ICommandHandler<UpdateEventCommand, UpdateEventResult>
{
    public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        @event.UpdateDetails(request.Title, request.Description, request.EventDate, request.Location);

        await eventRepository.UpdateAsync(@event, cancellationToken);

        return new UpdateEventResult(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.EventDate,
            @event.Location,
            @event.CreatedByUserId,
            @event.Status.ToString(),
            @event.CreatedAt);
    }
}
