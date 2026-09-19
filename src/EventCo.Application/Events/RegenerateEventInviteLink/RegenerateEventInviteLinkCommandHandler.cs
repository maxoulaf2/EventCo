using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Security;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.RegenerateEventInviteLink;

public sealed class RegenerateEventInviteLinkCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository) : ICommandHandler<RegenerateEventInviteLinkCommand, RegenerateEventInviteLinkResult>
{
    public async Task<RegenerateEventInviteLinkResult> Handle(RegenerateEventInviteLinkCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var newToken = SecureTokenGenerator.GenerateUrlSafeToken();
        @event.RegenerateInviteLink(currentUserService.UserId!.Value, newToken);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new RegenerateEventInviteLinkResult(@event.Id, @event.InviteLinkToken);
    }
}
