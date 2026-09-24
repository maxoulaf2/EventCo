using FluentValidation;

namespace EventCo.Application.Auth.VerifyMagicLink;

public sealed class VerifyMagicLinkCommandValidator : AbstractValidator<VerifyMagicLinkCommand>
{
    public VerifyMagicLinkCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{6}$")
            .WithMessage("Le code de connexion doit comporter 6 chiffres.");
    }
}
