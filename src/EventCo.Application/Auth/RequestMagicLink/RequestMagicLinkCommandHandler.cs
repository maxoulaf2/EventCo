using System.Security.Cryptography;
using System.Text;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Domain.Auth;
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
        var email = Email.Create(request.Email);
        var now = dateTimeProvider.UtcNow;
        var magicLinkOptions = options.Value;

        var rawToken = GenerateRawToken();
        var tokenHash = HashToken(rawToken);

        var token = MagicLinkToken.Create(email, tokenHash, now.AddMinutes(magicLinkOptions.ExpiryMinutes), now);

        await magicLinkTokenRepository.AddAsync(token, cancellationToken);

        var verificationLink = $"{magicLinkOptions.VerificationUrlBase}?token={Uri.EscapeDataString(rawToken)}";

        await emailSender.SendAsync(
            email.Value,
            "Votre lien de connexion EventCo",
            $"<p>Cliquez sur ce lien pour vous connecter à EventCo (valable {magicLinkOptions.ExpiryMinutes} minutes) :</p>"
            + $"<p><a href=\"{verificationLink}\">{verificationLink}</a></p>",
            cancellationToken);
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashToken(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
    }
}
