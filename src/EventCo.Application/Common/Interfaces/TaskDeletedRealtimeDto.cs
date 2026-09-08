namespace EventCo.Application.Common.Interfaces;

public sealed record TaskDeletedRealtimeDto(Guid EventId, Guid TaskId);
