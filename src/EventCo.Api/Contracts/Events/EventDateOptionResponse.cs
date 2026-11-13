namespace EventCo.Api.Contracts.Events;

public sealed record EventDateOptionResponse(Guid Id, DateTime Date, IReadOnlyList<Guid> VoterUserIds);
