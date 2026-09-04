using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetMyEvents;

public sealed record GetMyEventsQuery : ICommand<GetMyEventsResult>;
