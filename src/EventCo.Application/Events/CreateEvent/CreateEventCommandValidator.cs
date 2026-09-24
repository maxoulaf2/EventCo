using FluentValidation;

namespace EventCo.Application.Events.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.EventDate).NotEmpty();
    }
}
