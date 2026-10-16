using EventCo.Domain.Events;
using FluentValidation;

namespace EventCo.Application.Events.CorrectItemCategory;

public sealed class CorrectItemCategoryCommandValidator : AbstractValidator<CorrectItemCategoryCommand>
{
    public CorrectItemCategoryCommandValidator()
    {
        RuleFor(x => x.Category)
            .Must(category => category is not null && Enum.IsDefined(typeof(EventItemCategory), category))
            .WithMessage($"La catégorie de l'article doit être l'une des valeurs suivantes : {string.Join(", ", Enum.GetNames<EventItemCategory>())}.");
    }
}
