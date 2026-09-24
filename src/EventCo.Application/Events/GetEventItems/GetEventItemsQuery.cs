using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetEventItems;

public sealed record GetEventItemsQuery(Guid EventId) : ICommand<GetEventItemsResult>;
