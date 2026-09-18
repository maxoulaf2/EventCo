using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location,
    string? ImageUrl = null) : ICommand<CreateEventResult>;
