using FluentValidation;

namespace EventCo.Application.Events.InviteParticipant;

public sealed class InviteParticipantCommandValidator : AbstractValidator<InviteParticipantCommand>
{
    public InviteParticipantCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
