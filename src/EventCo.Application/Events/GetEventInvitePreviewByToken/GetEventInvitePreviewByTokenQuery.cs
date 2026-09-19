using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetEventInvitePreviewByToken;

public sealed record GetEventInvitePreviewByTokenQuery(string Token) : ICommand<EventInvitePreviewResult>;
