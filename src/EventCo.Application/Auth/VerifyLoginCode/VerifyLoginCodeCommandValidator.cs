using FluentValidation;

namespace EventCo.Application.Auth.VerifyLoginCode;

public sealed class VerifyLoginCodeCommandValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    public VerifyLoginCodeCommandValidator()
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
