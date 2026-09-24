using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;

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
        var email = Email.From(request.Email);

        var usableTokens = await magicLinkTokenRepository.GetUsableByEmailAsync(email, now, cancellationToken);
        var token = usableTokens.FirstOrDefault(t => MagicLinkTokenHasher.Matches(request.Code, t.TokenHash));

        if (token is null)
        {
            // L'essai raté est compté sur chaque code encore utilisable de cet email : sans ça, redemander
            // des codes permettrait de multiplier les essais autorisés sur un même code.
            foreach (var usableToken in usableTokens)
            {
                usableToken.RegisterFailedAttempt();
                await magicLinkTokenRepository.ApplyAsync(usableToken, cancellationToken);
            }

            return new VerifyMagicLinkResult.Invalid();
        }

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

        return new VerifyMagicLinkResult.Succeeded(user.Id, user.Email.Value, user.DisplayName, session.Value, session.ExpiresAt, eventId);
    }

    private static string DisplayNameFromEmail(string email) => email[..email.IndexOf('@')];
}
