using EventCo.Api.Contracts.Events;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.GetEventById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/events")]
public sealed class EventsController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(
            new CreateEventCommand(request.Title, request.Description, request.EventDate, request.Location),
            cancellationToken);

        var response = new EventResponse(
            result.EventId,
            result.Title,
            result.Description,
            result.EventDate,
            result.Location,
            result.CreatedByUserId,
            result.Status,
            result.CreatedAt);

        return Created($"api/events/{result.EventId}", response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetEventByIdQuery(id), cancellationToken);

        var response = new EventResponse(
            result.EventId,
            result.Title,
            result.Description,
            result.EventDate,
            result.Location,
            result.CreatedByUserId,
            result.Status,
            result.CreatedAt);

        return Ok(response);
    }
}
