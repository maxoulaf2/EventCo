using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.CorrectItemCategory;

public sealed record CorrectItemCategoryCommand(Guid EventId, Guid ItemId, string Category) : ICommand;
