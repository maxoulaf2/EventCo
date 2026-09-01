using FluentValidation;

namespace EventCo.Application.Auth.VerifyMagicLink;

public sealed class VerifyMagicLinkCommandValidator : AbstractValidator<VerifyMagicLinkCommand>
{
    public VerifyMagicLinkCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
