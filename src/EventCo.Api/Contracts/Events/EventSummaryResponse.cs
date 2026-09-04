namespace EventCo.Api.Contracts.Events;

public sealed record EventSummaryResponse(
    Guid Id,
    string Title,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    string Role,
    bool HasJoined);
