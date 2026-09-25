using EventCo.Domain.Events;
using FluentValidation;

namespace EventCo.Application.Events.CreateItem;

public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();

        RuleFor(x => x.Kind)
            .Must(kind => kind is not null && Enum.IsDefined(typeof(EventItemKind), kind))
            .WithMessage("La nature de l'article doit être l'une des valeurs suivantes : ToBring, Contribution.");
    }
}
