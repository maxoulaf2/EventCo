namespace EventCo.Api.Contracts.Events;

public sealed record RegenerateEventInviteLinkResponse(Guid EventId, string InviteLinkToken);
