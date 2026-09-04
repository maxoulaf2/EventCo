using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetEventById;

public sealed record GetEventByIdQuery(Guid EventId) : ICommand<GetEventByIdResult>;
