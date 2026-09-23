using EventCo.Api.Auth;
using EventCo.Api.Contracts.Auth;
using EventCo.Application.Auth.GetCurrentUser;
using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Auth.UpdateProfile;
using EventCo.Application.Auth.VerifyMagicLink;
using EventCo.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace EventCo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ICommandDispatcher commandDispatcher, IHostEnvironment environment) : ControllerBase
{
    [HttpPost("request-link")]
    public async Task<IActionResult> RequestLink(RequestMagicLinkRequest request, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new RequestMagicLinkCommand(request.Email, request.EventInviteLinkToken), cancellationToken);

        return Accepted();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyMagicLinkRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new VerifyMagicLinkCommand(request.Token), cancellationToken);

        // Same-origin en dev (proxy Vite, cf. client/vite.config.ts) comme en prod (frontend/API
        // servis sous le même domaine) : SameSite=Lax suffit et évite que le cookie soit traité comme
        // cookie tiers (bloqué par défaut en navigation privée). Secure reste désactivé en Development
        // car la connexion navigateur -> Vite y est en http.
        Response.Cookies.Append(SessionCookie.Name, result.SessionToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = result.SessionExpiresAt,
        });

        return Ok(new VerifyMagicLinkResponse(result.UserId, result.Email, result.DisplayName, result.EventId));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetCurrentUserQuery(), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName, result.IsAdmin));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new UpdateProfileCommand(request.DisplayName), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName, result.IsAdmin));
    }

    // Le token de session est auto-porté (cf. SessionTokenService) : la déconnexion ne fait que
    // supprimer le cookie côté navigateur, sans registre de révocation côté serveur.
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(SessionCookie.Name, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
        });

        return NoContent();
    }
}
