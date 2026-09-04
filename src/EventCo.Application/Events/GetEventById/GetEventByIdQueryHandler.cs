using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.GetEventById;

public sealed class GetEventByIdQueryHandler(IEventRepository eventRepository)
    : ICommandHandler<GetEventByIdQuery, GetEventByIdResult>
{
    public async Task<GetEventByIdResult> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        return new GetEventByIdResult(
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
