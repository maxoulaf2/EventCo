using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.UnassignItem;

public sealed record UnassignItemCommand(Guid EventId, Guid ItemId) : ICommand;
