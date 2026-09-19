using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.JoinEventViaInviteLink;

public sealed class JoinEventViaInviteLinkCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<JoinEventViaInviteLinkCommand, JoinEventViaInviteLinkResult>
{
    public async Task<JoinEventViaInviteLinkResult> Handle(JoinEventViaInviteLinkCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByInviteLinkTokenAsync(request.Token, cancellationToken)
            ?? throw new InviteLinkNotFoundException(request.Token);

        @event.JoinViaInviteLink(currentUserService.UserId!.Value, dateTimeProvider.UtcNow);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new JoinEventViaInviteLinkResult(@event.Id);
    }
}
