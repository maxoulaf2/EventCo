using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.DeleteItem;

public sealed record DeleteItemCommand(Guid EventId, Guid ItemId) : ICommand;
