using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid EventId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? Location) : ICommand<UpdateEventResult>;
