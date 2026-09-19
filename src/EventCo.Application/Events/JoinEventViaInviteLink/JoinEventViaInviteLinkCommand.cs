using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.JoinEventViaInviteLink;

public sealed record JoinEventViaInviteLinkCommand(string Token) : ICommand<JoinEventViaInviteLinkResult>;
