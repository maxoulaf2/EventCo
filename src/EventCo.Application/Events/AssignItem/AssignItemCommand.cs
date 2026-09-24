using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.AssignItem;

public sealed record AssignItemCommand(Guid EventId, Guid ItemId, Guid UserId) : ICommand;
