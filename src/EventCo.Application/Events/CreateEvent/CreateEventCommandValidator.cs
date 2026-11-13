using FluentValidation;

namespace EventCo.Application.Events.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.EventDate)
            .NotEmpty()
            .When(x => x.DateOptions is not { Count: > 0 })
            .WithMessage("Indiquez une date, ou proposez plusieurs dates au vote.");

        RuleFor(x => x.DateOptions)
            .Empty()
            .When(x => x.EventDate is not null)
            .WithMessage("Un événement a soit une date fixe, soit un sondage de date, pas les deux.");
    }
}
