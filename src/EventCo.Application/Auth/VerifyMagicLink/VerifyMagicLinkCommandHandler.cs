using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.Users;

namespace EventCo.Application.Auth.VerifyMagicLink;

public sealed class VerifyMagicLinkCommandHandler(
    IMagicLinkTokenRepository magicLinkTokenRepository,
    IUserRepository userRepository,
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
        await magicLinkTokenRepository.UpdateAsync(token, cancellationToken);

        var user = await userRepository.GetByEmailAsync(token.Email, cancellationToken);
        if (user is null)
        {
            user = User.Create(token.Email, DisplayNameFromEmail(token.Email.Value), now);
            await userRepository.AddAsync(user, cancellationToken);
        }

        var session = sessionTokenService.CreateSessionToken(user.Id, user.Email.Value, now);

        return new VerifyMagicLinkResult(user.Id, user.Email.Value, user.DisplayName, session.Value, session.ExpiresAt);
    }

    private static string DisplayNameFromEmail(string email) => email[..email.IndexOf('@')];
}
