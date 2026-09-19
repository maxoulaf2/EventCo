using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.GetEventById;

public sealed class GetEventByIdQueryHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IUserRepository userRepository)
    : ICommandHandler<GetEventByIdQuery, GetEventByIdResult>
{
    public async Task<GetEventByIdResult> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var currentUserId = currentUserService.UserId!.Value;
        @event.EnsureCanBeViewedBy(currentUserId);

        var isCreatorOrOrganizer = @event.CreatedByUserId == currentUserId
            || @event.Participants.Any(p => p.UserId == currentUserId && p.Role == ParticipantRole.Organizer);
        var inviteLinkToken = isCreatorOrOrganizer ? @event.InviteLinkToken : null;

        var userIds = @event.Participants.Select(p => p.UserId).ToList();
        var users = await userRepository.GetByIdsAsync(userIds, cancellationToken);
        var usersById = users.ToDictionary(u => u.Id);

        var participants = @event.Participants
            .Select(participant =>
            {
                var user = usersById[participant.UserId];
                return new EventParticipantSummary(
                    user.Id,
                    user.Email.Value,
                    user.DisplayName,
                    participant.Role.ToString(),
                    participant.InvitedAt,
                    participant.ParticipationStatus.ToString());
            })
            .ToList();

        return new GetEventByIdResult(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.EventDate,
            @event.Location,
            @event.ImageUrl,
            @event.CreatedByUserId,
            @event.Status.ToString(),
            @event.CreatedAt,
            participants,
            inviteLinkToken);
    }
}
