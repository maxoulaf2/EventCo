using EventCo.Api.Auth;
using EventCo.Api.Contracts.Auth;
using EventCo.Application.Auth.GetCurrentUser;
using EventCo.Application.Auth.RemoveAvatar;
using EventCo.Application.Auth.RequestLoginCode;
using EventCo.Application.Auth.UpdateAvatar;
using EventCo.Application.Auth.UpdateProfile;
using EventCo.Application.Auth.VerifyLoginCode;
using EventCo.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace EventCo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ICommandDispatcher commandDispatcher, IHostEnvironment environment) : ControllerBase
{
    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode(RequestLoginCodeRequest request, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new RequestLoginCodeCommand(request.Email, request.EventInviteLinkToken), cancellationToken);

        return Accepted();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyLoginCodeRequest request, CancellationToken cancellationToken)
    {
        var verifyResult = await commandDispatcher.Send(new VerifyLoginCodeCommand(request.Email, request.Code), cancellationToken);

        // Message volontairement unique (code faux, expiré, déjà utilisé ou bloqué) : ne rien révéler
        // de l'état des codes de cet email.
        if (verifyResult is not VerifyLoginCodeResult.Succeeded result)
            return Problem(
                title: "Code invalide",
                detail: "Ce code est invalide ou a expiré.",
                statusCode: StatusCodes.Status400BadRequest);

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

        return Ok(new VerifyLoginCodeResponse(result.UserId, result.Email, result.DisplayName, result.EventId));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetCurrentUserQuery(), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName, result.IsAdmin, result.AvatarUrl));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new UpdateProfileCommand(request.DisplayName), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName, result.IsAdmin, result.AvatarUrl));
    }

    // multipart/form-data, champ "file". Taille et format validés par UpdateAvatarCommandValidator ;
    // la limite de requête ci-dessous (marge pour l'enveloppe multipart) coupe court avant de charger en
    // mémoire un fichier manifestement trop gros.
    [Authorize]
    [HttpPut("me/avatar")]
    [RequestSizeLimit(UpdateAvatarCommandValidator.MaxContentBytes + 64 * 1024)]
    public async Task<IActionResult> UpdateAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content, cancellationToken);

        var result = await commandDispatcher.Send(new UpdateAvatarCommand(content.ToArray()), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName, result.IsAdmin, result.AvatarUrl));
    }

    [Authorize]
    [HttpDelete("me/avatar")]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new RemoveAvatarCommand(), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName, result.IsAdmin, result.AvatarUrl));
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
