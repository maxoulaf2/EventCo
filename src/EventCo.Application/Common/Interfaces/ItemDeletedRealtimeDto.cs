namespace EventCo.Application.Common.Interfaces;

public sealed record ItemDeletedRealtimeDto(Guid EventId, Guid ItemId);
