using EventCo.Api.Contracts.Events;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignItem;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateItem;
using EventCo.Application.Events.DeleteEvent;
using EventCo.Application.Events.DeleteItem;
using EventCo.Application.Events.DemoteToParticipant;
using EventCo.Application.Events.GetEventById;
using EventCo.Application.Events.GetEventInvitePreviewByToken;
using EventCo.Application.Events.GetEventItems;
using EventCo.Application.Events.GetMyEvents;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.JoinEventViaInviteLink;
using EventCo.Application.Events.PromoteToOrganizer;
using EventCo.Application.Events.RegenerateEventInviteLink;
using EventCo.Application.Events.SetParticipationStatus;
using EventCo.Application.Events.UnassignItem;
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
            new CreateEventCommand(request.Title, request.Description, request.EventDate, request.Location, request.ImageUrl),
            cancellationToken);

        var response = new EventResponse(
            result.EventId,
            result.Title,
            result.Description,
            result.EventDate,
            result.Location,
            result.ImageUrl,
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
            result.ImageUrl,
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
                p.ParticipationStatus,
                p.AvatarUrl)).ToList(),
            result.InviteLinkToken);

        return Ok(response);
    }

    [HttpPost("{id:guid}/invite-link/regenerate")]
    public async Task<IActionResult> RegenerateInviteLink(Guid id, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new RegenerateEventInviteLinkCommand(id), cancellationToken);

        return Ok(new RegenerateEventInviteLinkResponse(result.EventId, result.InviteLinkToken));
    }

    [AllowAnonymous]
    [HttpGet("invite-links/{token}/preview")]
    public async Task<IActionResult> GetInviteLinkPreview(string token, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetEventInvitePreviewByTokenQuery(token), cancellationToken);

        return Ok(new EventInvitePreviewResponse(result.EventId, result.Title, result.EventDate, result.Location, result.CreatedByDisplayName));
    }

    [HttpPost("invite-links/{token}/join")]
    public async Task<IActionResult> JoinViaInviteLink(string token, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new JoinEventViaInviteLinkCommand(token), cancellationToken);

        return Ok(new JoinEventViaInviteLinkResponse(result.EventId));
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
            result.ImageUrl,
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
            result.ParticipationStatus,
            result.AvatarUrl);

        return Created($"api/events/{id}", response);
    }

    [HttpGet("{id:guid}/items")]
    public async Task<IActionResult> GetItems(Guid id, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(new GetEventItemsQuery(id), cancellationToken);

        var response = result.Items.Select(t => new EventItemResponse(
            t.ItemId,
            t.EventId,
            t.Title,
            t.Quantity,
            t.AssignedToUserId,
            t.CreatedAt));

        return Ok(response);
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> CreateItem(Guid id, CreateItemRequest request, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send(
            new CreateItemCommand(id, request.Title, request.Quantity),
            cancellationToken);

        var response = new EventItemResponse(
            result.ItemId,
            result.EventId,
            result.Title,
            result.Quantity,
            result.AssignedToUserId,
            result.CreatedAt);

        return Created($"api/events/{id}/items/{result.ItemId}", response);
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/assign/{userId:guid}")]
    public async Task<IActionResult> AssignItem(Guid id, Guid itemId, Guid userId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new AssignItemCommand(id, itemId, userId), cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/unassign")]
    public async Task<IActionResult> UnassignItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new UnassignItemCommand(id, itemId), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        await commandDispatcher.Send(new DeleteItemCommand(id, itemId), cancellationToken);

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
