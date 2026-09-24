using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.UpdateEventImage;

public sealed record UpdateEventImageCommand(Guid EventId, byte[] Content) : ICommand<UpdateEventImageResult>;
