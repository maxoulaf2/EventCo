using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;

namespace EventCo.Application.Auth.VerifyLoginCode;

public sealed class VerifyLoginCodeCommandHandler(
    ILoginCodeRepository loginCodeRepository,
    IUserRepository userRepository,
    IEventRepository eventRepository,
    ISessionTokenService sessionTokenService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<VerifyLoginCodeCommand, VerifyLoginCodeResult>
{
    public async Task<VerifyLoginCodeResult> Handle(VerifyLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var email = Email.From(request.Email);

        var usableLoginCodes = await loginCodeRepository.GetUsableByEmailAsync(email, now, cancellationToken);
        var loginCode = usableLoginCodes.FirstOrDefault(t => LoginCodeHasher.Matches(request.Code, t.CodeHash));

        if (loginCode is null)
        {
            // L'essai raté est compté sur chaque code encore utilisable de cet email : sans ça, redemander
            // des codes permettrait de multiplier les essais autorisés sur un même code.
            foreach (var usableLoginCode in usableLoginCodes)
            {
                usableLoginCode.RegisterFailedAttempt();
                await loginCodeRepository.ApplyAsync(usableLoginCode, cancellationToken);
            }

            return new VerifyLoginCodeResult.Invalid();
        }

        loginCode.Consume(now);
        await loginCodeRepository.ApplyAsync(loginCode, cancellationToken);

        var user = await userRepository.GetByEmailAsync(loginCode.Email, cancellationToken);
        if (user is null)
        {
            user = User.Create(loginCode.Email, DisplayNameFromEmail(loginCode.Email.Value), now);
            await userRepository.ApplyAsync(user, cancellationToken);
        }

        var session = sessionTokenService.CreateSessionToken(user.Id, user.Email.Value, now);

        Guid? eventId = null;
        if (loginCode.EventInviteLinkToken is not null)
        {
            var joinedEvent = await eventRepository.GetByInviteLinkTokenAsync(loginCode.EventInviteLinkToken, cancellationToken);
            if (joinedEvent is not null)
            {
                joinedEvent.JoinViaInviteLink(user.Id, now);
                await eventRepository.ApplyAsync(joinedEvent, cancellationToken);
                eventId = joinedEvent.Id;
            }
            // joinedEvent null (lien régénéré entre-temps) : on ignore silencieusement, la connexion reste valide.
        }

        return new VerifyLoginCodeResult.Succeeded(user.Id, user.Email.Value, user.DisplayName, session.Value, session.ExpiresAt, eventId);
    }

    private static string DisplayNameFromEmail(string email) => email[..email.IndexOf('@')];
}
