namespace EventCo.Application.Events.RegenerateEventInviteLink;

public sealed record RegenerateEventInviteLinkResult(Guid EventId, string InviteLinkToken);
