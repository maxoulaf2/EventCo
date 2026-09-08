using EventCo.Domain.Events;
using FluentValidation;

namespace EventCo.Application.Events.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Category)
            .Must(category => Enum.TryParse<TaskCategory>(category, out _))
            .WithMessage("La catégorie doit être l'une des valeurs suivantes : Courses, Logistique, Autre.");
    }
}
