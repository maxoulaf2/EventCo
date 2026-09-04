using EventCo.Api.Contracts.Events;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.DeleteEvent;
using EventCo.Application.Events.GetEventById;
using EventCo.Application.Events.GetMyEvents;
using EventCo.Application.Events.UpdateEvent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/events")]
public sealed class EventsController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetMyEventsQuery(), cancellationToken);

        var response = result.Events.Select(e => new EventSummaryResponse(
            e.EventId,
            e.Title,
            e.EventDate,
            e.Location,
            e.CreatedByUserId,
            e.Status,
            e.Role,
            e.HasJoined));

        return Ok(response);
    }

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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(
            new UpdateEventCommand(id, request.Title, request.Description, request.EventDate, request.Location),
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

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new DeleteEventCommand(id), cancellationToken);

        return NoContent();
    }
}
