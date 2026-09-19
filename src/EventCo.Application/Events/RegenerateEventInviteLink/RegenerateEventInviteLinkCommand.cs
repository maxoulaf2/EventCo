using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.RegenerateEventInviteLink;

public sealed record RegenerateEventInviteLinkCommand(Guid EventId) : ICommand<RegenerateEventInviteLinkResult>;
