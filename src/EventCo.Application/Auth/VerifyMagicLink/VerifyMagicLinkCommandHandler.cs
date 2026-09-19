using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.Users;

namespace EventCo.Application.Auth.VerifyMagicLink;

public sealed class VerifyMagicLinkCommandHandler(
    IMagicLinkTokenRepository magicLinkTokenRepository,
    IUserRepository userRepository,
    IEventRepository eventRepository,
    ISessionTokenService sessionTokenService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<VerifyMagicLinkCommand, VerifyMagicLinkResult>
{
    public async Task<VerifyMagicLinkResult> Handle(VerifyMagicLinkCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var tokenHash = MagicLinkTokenHasher.Hash(request.Token);

        var token = await magicLinkTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new MagicLinkTokenNotFoundException();

        token.Consume(now);
        await magicLinkTokenRepository.ApplyAsync(token, cancellationToken);

        var user = await userRepository.GetByEmailAsync(token.Email, cancellationToken);
        if (user is null)
        {
            user = User.Create(token.Email, DisplayNameFromEmail(token.Email.Value), now);
            await userRepository.ApplyAsync(user, cancellationToken);
        }

        var session = sessionTokenService.CreateSessionToken(user.Id, user.Email.Value, now);

        Guid? eventId = null;
        if (token.EventInviteLinkToken is not null)
        {
            var joinedEvent = await eventRepository.GetByInviteLinkTokenAsync(token.EventInviteLinkToken, cancellationToken);
            if (joinedEvent is not null)
            {
                joinedEvent.JoinViaInviteLink(user.Id, now);
                await eventRepository.ApplyAsync(joinedEvent, cancellationToken);
                eventId = joinedEvent.Id;
            }
            // joinedEvent null (lien régénéré entre-temps) : on ignore silencieusement, la connexion reste valide.
        }

        return new VerifyMagicLinkResult(user.Id, user.Email.Value, user.DisplayName, session.Value, session.ExpiresAt, eventId);
    }

    private static string DisplayNameFromEmail(string email) => email[..email.IndexOf('@')];
}
