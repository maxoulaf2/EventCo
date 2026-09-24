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

        var code = SecureTokenGenerator.GenerateNumericCode();
        var codeHash = MagicLinkTokenHasher.Hash(code);

        var token = MagicLinkToken.Create(email, codeHash, now.AddMinutes(magicLinkOptions.ExpiryMinutes), now, request.EventInviteLinkToken);

        await magicLinkTokenRepository.ApplyAsync(token, cancellationToken);

        await emailSender.SendAsync(
            email.Value,
            $"{code} est votre code de connexion EventCo",
            $"<p>Voici votre code de connexion à EventCo (valable {magicLinkOptions.ExpiryMinutes} minutes) :</p>"
            + $"<p style=\"font-size:32px;font-weight:bold;letter-spacing:6px\">{code}</p>"
            + "<p>Si vous n'êtes pas à l'origine de cette demande, ignorez simplement cet email.</p>",
            cancellationToken);
    }
}
