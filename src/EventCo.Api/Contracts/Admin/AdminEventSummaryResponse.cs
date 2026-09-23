namespace EventCo.Api.Contracts.Admin;

public sealed record AdminEventSummaryResponse(
    Guid Id,
    string Title,
    DateTime EventDate,
    string? Location,
    Guid CreatedByUserId,
    string Status,
    int ParticipantCount);
