using EventCo.Api.Auth;
using EventCo.Api.Contracts.Auth;
using EventCo.Application.Auth.GetCurrentUser;
using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Auth.VerifyMagicLink;
using EventCo.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost("request-link")]
    public async Task<IActionResult> RequestLink(RequestMagicLinkRequest request, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new RequestMagicLinkCommand(request.Email), cancellationToken);

        return Accepted();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyMagicLinkRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new VerifyMagicLinkCommand(request.Token), cancellationToken);

        Response.Cookies.Append(SessionCookie.Name, result.SessionToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = result.SessionExpiresAt,
        });

        return Ok(new VerifyMagicLinkResponse(result.UserId, result.Email, result.DisplayName));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetCurrentUserQuery(), cancellationToken);

        return Ok(new CurrentUserResponse(result.UserId, result.Email, result.DisplayName));
    }
}
