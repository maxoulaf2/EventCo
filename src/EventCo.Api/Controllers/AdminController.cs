using EventCo.Api.Contracts.Admin;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.GetAllEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCo.Api.Controllers;

// [Authorize] ne vérifie que l'authentification : le contrôle du flag administrateur est fait par
// chaque use case (UserNotAdminException -> 403), comme les autres règles d'autorisation métier.
[Authorize]
[ApiController]
[Route("api/admin")]
public sealed class AdminController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpGet("events")]
    public async Task<IActionResult> GetAllEvents(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetAllEventsQuery(), cancellationToken);

        var response = result.Events.Select(e => new AdminEventSummaryResponse(
            e.EventId,
            e.Title,
            e.EventDate,
            e.Location,
            e.CreatedByUserId,
            e.Status,
            e.ParticipantCount));

        return Ok(response);
    }
}
