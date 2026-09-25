using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.CreateItem;

public sealed record CreateItemCommand(Guid EventId, string Title, string? Quantity, string Kind) : ICommand<CreateItemResult>;
