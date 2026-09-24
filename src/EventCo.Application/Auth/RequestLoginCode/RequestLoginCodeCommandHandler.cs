using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Application.Common.Security;
using EventCo.Domain.Auth;
using EventCo.Domain.Auth.Exceptions;
using EventCo.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace EventCo.Application.Auth.RequestLoginCode;

public sealed class RequestLoginCodeCommandHandler(
    ILoginCodeRepository loginCodeRepository,
    IEmailSender emailSender,
    IDateTimeProvider dateTimeProvider,
    IOptions<LoginCodeOptions> options) : ICommandHandler<RequestLoginCodeCommand>
{
    public async Task Handle(RequestLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var email = Email.From(request.Email);
        var now = dateTimeProvider.UtcNow;
        var loginCodeOptions = options.Value;

        var windowStart = now.AddMinutes(-loginCodeOptions.RateLimitWindowMinutes);
        var recentRequestCount = await loginCodeRepository.CountCreatedSinceAsync(email, windowStart, cancellationToken);
        if (recentRequestCount >= loginCodeOptions.MaxRequestsPerWindow)
            throw new TooManyLoginCodeRequestsException(email.Value, loginCodeOptions.MaxRequestsPerWindow, loginCodeOptions.RateLimitWindowMinutes);

        var code = SecureTokenGenerator.GenerateNumericCode();
        var codeHash = LoginCodeHasher.Hash(code);

        var loginCode = LoginCode.Create(email, codeHash, now.AddMinutes(loginCodeOptions.ExpiryMinutes), now, request.EventInviteLinkToken);

        await loginCodeRepository.ApplyAsync(loginCode, cancellationToken);

        await emailSender.SendAsync(
            email.Value,
            $"{code} est votre code de connexion EventCo",
            $"<p>Voici votre code de connexion à EventCo (valable {loginCodeOptions.ExpiryMinutes} minutes) :</p>"
            + $"<p style=\"font-size:32px;font-weight:bold;letter-spacing:6px\">{code}</p>"
            + "<p>Si vous n'êtes pas à l'origine de cette demande, ignorez simplement cet email.</p>",
            cancellationToken);
    }
}
