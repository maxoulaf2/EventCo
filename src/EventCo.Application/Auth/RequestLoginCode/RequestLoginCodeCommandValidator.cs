using FluentValidation;

namespace EventCo.Application.Auth.RequestLoginCode;

public sealed class RequestLoginCodeCommandValidator : AbstractValidator<RequestLoginCodeCommand>
{
    public RequestLoginCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
