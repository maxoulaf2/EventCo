using EventCo.Api.Contracts.Auth;
using EventCo.Application.Auth.RequestMagicLink;
using EventCo.Application.Common.Messaging;
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
}
