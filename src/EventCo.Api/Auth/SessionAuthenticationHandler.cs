using System.Security.Claims;
using System.Text.Encodings.Web;
using EventCo.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EventCo.Api.Auth;

// Lit le cookie de session (SessionCookie.Name) et le valide via ISessionTokenService plutôt que de
// s'appuyer sur un scheme cookie ASP.NET Core standard : le token n'est pas un ticket chiffré par
// DataProtection mais le format maison payload+signature HMAC produit par VerifyMagicLinkCommand.
public sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISessionTokenService sessionTokenService,
    IDateTimeProvider dateTimeProvider)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(SessionCookie.Name, out var token) || string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var session = sessionTokenService.ValidateSessionToken(token, dateTimeProvider.UtcNow);
        if (session is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Email, session.Email),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
