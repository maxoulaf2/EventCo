using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;

namespace EventCo.Application.Events.InviteParticipant;

public sealed class InviteParticipantCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<InviteParticipantCommand, InviteParticipantResult>
{
    public async Task<InviteParticipantResult> Handle(InviteParticipantCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var email = Email.Create(request.Email);
        var now = dateTimeProvider.UtcNow;

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            user = User.Create(email, DisplayNameFromEmail(email.Value), now);
            await userRepository.ApplyAsync(user, cancellationToken);
        }

        var participant = @event.InviteParticipant(currentUserService.UserId!.Value, user.Id, now);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new InviteParticipantResult(
            @event.Id,
            user.Id,
            user.Email.Value,
            user.DisplayName,
            participant.Role.ToString(),
            participant.InvitedAt,
            participant.HasJoined);
    }

    private static string DisplayNameFromEmail(string email) => email[..email.IndexOf('@')];
}
