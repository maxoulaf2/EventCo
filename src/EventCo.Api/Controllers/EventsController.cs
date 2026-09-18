using EventCo.Api.Contracts.Events;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignTask;
using EventCo.Application.Events.CompleteTask;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.DeleteEvent;
using EventCo.Application.Events.DeleteTask;
using EventCo.Application.Events.DemoteToParticipant;
using EventCo.Application.Events.GetEventById;
using EventCo.Application.Events.GetEventTasks;
using EventCo.Application.Events.GetMyEvents;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.PromoteToOrganizer;
using EventCo.Application.Events.ReopenTask;
using EventCo.Application.Events.SetParticipationStatus;
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
            e.Role));

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

        var response = new EventDetailResponse(
            result.EventId,
            result.Title,
            result.Description,
            result.EventDate,
            result.Location,
            result.CreatedByUserId,
            result.Status,
            result.CreatedAt,
            result.Participants.Select(p => new EventParticipantResponse(
                id,
                p.UserId,
                p.Email,
                p.DisplayName,
                p.Role,
                p.InvitedAt,
                p.ParticipationStatus)).ToList());

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

    [HttpPost("{id:guid}/participants")]
    public async Task<IActionResult> InviteParticipant(Guid id, InviteParticipantRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(
            new InviteParticipantCommand(id, request.Email),
            cancellationToken);

        var response = new EventParticipantResponse(
            result.EventId,
            result.UserId,
            result.Email,
            result.DisplayName,
            result.Role,
            result.InvitedAt,
            result.ParticipationStatus);

        return Created($"api/events/{id}", response);
    }

    [HttpGet("{id:guid}/tasks")]
    public async Task<IActionResult> GetTasks(Guid id, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetEventTasksQuery(id), cancellationToken);

        var response = result.Tasks.Select(t => new EventTaskResponse(
            t.TaskId,
            t.EventId,
            t.Title,
            t.Category,
            t.Quantity,
            t.AssignedToUserId,
            t.IsDone,
            t.CreatedAt));

        return Ok(response);
    }

    [HttpPost("{id:guid}/tasks")]
    public async Task<IActionResult> CreateTask(Guid id, CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(
            new CreateTaskCommand(id, request.Title, request.Category, request.Quantity),
            cancellationToken);

        var response = new EventTaskResponse(
            result.TaskId,
            result.EventId,
            result.Title,
            result.Category,
            result.Quantity,
            result.AssignedToUserId,
            result.IsDone,
            result.CreatedAt);

        return Created($"api/events/{id}/tasks/{result.TaskId}", response);
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}/assign/{userId:guid}")]
    public async Task<IActionResult> AssignTask(Guid id, Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new AssignTaskCommand(id, taskId, userId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}/complete")]
    public async Task<IActionResult> CompleteTask(Guid id, Guid taskId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new CompleteTaskCommand(id, taskId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}/reopen")]
    public async Task<IActionResult> ReopenTask(Guid id, Guid taskId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new ReopenTaskCommand(id, taskId), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, Guid taskId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new DeleteTaskCommand(id, taskId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/participants/{userId:guid}/promote")]
    public async Task<IActionResult> PromoteParticipant(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new PromoteToOrganizerCommand(id, userId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/participants/{userId:guid}/demote")]
    public async Task<IActionResult> DemoteParticipant(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new DemoteToParticipantCommand(id, userId), cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/participation-status")]
    public async Task<IActionResult> SetParticipationStatus(Guid id, SetParticipationStatusRequest request, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new SetParticipationStatusCommand(id, request.Status), cancellationToken);

        return NoContent();
    }
}
