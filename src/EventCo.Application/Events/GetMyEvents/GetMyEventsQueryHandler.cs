using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetMyEvents;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class GetMyEventsQueryHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository) : ICommandHandler<GetMyEventsQuery, GetMyEventsResult>
{
    public async Task<GetMyEventsResult> Handle(GetMyEventsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId!.Value;
        var events = await eventRepository.GetByParticipantUserIdAsync(userId, cancellationToken);

        var summaries = events
            .Select(@event =>
            {
                var participant = @event.Participants.Single(p => p.UserId == userId);

                return new MyEventSummary(
                    @event.Id,
                    @event.Title,
                    @event.EventDate,
                    @event.Location,
                    @event.CreatedByUserId,
                    @event.Status.ToString(),
                    participant.Role.ToString(),
                    participant.HasJoined);
            })
            .ToList();

        return new GetMyEventsResult(summaries);
    }
}
