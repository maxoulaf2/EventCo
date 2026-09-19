using FluentValidation;

namespace EventCo.Application.Events.JoinEventViaInviteLink;

public sealed class JoinEventViaInviteLinkCommandValidator : AbstractValidator<JoinEventViaInviteLinkCommand>
{
    public JoinEventViaInviteLinkCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
