using FluentValidation;

namespace EventCo.Application.Events.CreateItem;

public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}
