using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.DeleteEvent;

public sealed record DeleteEventCommand(Guid EventId) : ICommand;
