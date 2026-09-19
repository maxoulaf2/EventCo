using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.GetEventInvitePreviewByToken;

// Accessible avant authentification (lien partagé) : n'expose volontairement que le strict
// nécessaire pour un aperçu (pas de liste de participants, pas de description/image), à la
// différence de GetEventByIdQuery qui suppose un utilisateur déjà participant.
public sealed class GetEventInvitePreviewByTokenQueryHandler(
    IEventRepository eventRepository,
    IUserRepository userRepository) : ICommandHandler<GetEventInvitePreviewByTokenQuery, EventInvitePreviewResult>
{
    public async Task<EventInvitePreviewResult> Handle(GetEventInvitePreviewByTokenQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByInviteLinkTokenAsync(request.Token, cancellationToken)
            ?? throw new InviteLinkNotFoundException(request.Token);

        var creator = await userRepository.GetByIdAsync(@event.CreatedByUserId, cancellationToken);

        return new EventInvitePreviewResult(
            @event.Id,
            @event.Title,
            @event.EventDate,
            @event.Location,
            creator!.DisplayName);
    }
}
