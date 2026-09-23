using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetAllEvents;

public sealed record GetAllEventsQuery : ICommand<GetAllEventsResult>;
