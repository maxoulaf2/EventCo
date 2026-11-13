using FluentValidation;

namespace EventCo.Application.Events.AddDateOption;

public sealed class AddDateOptionCommandValidator : AbstractValidator<AddDateOptionCommand>
{
    public AddDateOptionCommandValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
    }
}
