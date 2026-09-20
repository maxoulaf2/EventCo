using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Common.Security;
using EventCo.Domain.Auth;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace EventCo.Application.Auth.RequestMagicLink;

public sealed class RequestMagicLinkCommandHandler(
    IMagicLinkTokenRepository magicLinkTokenRepository,
    IEmailSender emailSender,
    IDateTimeProvider dateTimeProvider,
    IOptions<MagicLinkOptions> options) : ICommandHandler<RequestMagicLinkCommand>
{
    public async Task Handle(RequestMagicLinkCommand request, CancellationToken cancellationToken)
    {
        var email = Email.From(request.Email);
        var now = dateTimeProvider.UtcNow;
        var magicLinkOptions = options.Value;

        var windowStart = now.AddMinutes(-magicLinkOptions.RateLimitWindowMinutes);
        var recentRequestCount = await magicLinkTokenRepository.CountCreatedSinceAsync(email, windowStart, cancellationToken);
        if (recentRequestCount >= magicLinkOptions.MaxRequestsPerWindow)
            throw new TooManyMagicLinkRequestsException(email.Value, magicLinkOptions.MaxRequestsPerWindow, magicLinkOptions.RateLimitWindowMinutes);

        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        var tokenHash = MagicLinkTokenHasher.Hash(rawToken);

        var token = MagicLinkToken.Create(email, tokenHash, now.AddMinutes(magicLinkOptions.ExpiryMinutes), now, request.EventInviteLinkToken);

        await magicLinkTokenRepository.ApplyAsync(token, cancellationToken);

        var verificationLink = $"{magicLinkOptions.VerificationUrlBase}?token={Uri.EscapeDataString(rawToken)}";

        await emailSender.SendAsync(
            email.Value,
            "Votre lien de connexion EventCo",
            $"<p>Cliquez sur ce lien pour vous connecter à EventCo (valable {magicLinkOptions.ExpiryMinutes} minutes) :</p>"
            + $"<p><a href=\"{verificationLink}\">{verificationLink}</a></p>",
            cancellationToken);
    }
}
