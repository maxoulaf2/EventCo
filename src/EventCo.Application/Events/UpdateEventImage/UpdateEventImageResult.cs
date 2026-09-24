namespace EventCo.Application.Events.UpdateEventImage;

public sealed record UpdateEventImageResult(Guid EventId, string ImageUrl);
