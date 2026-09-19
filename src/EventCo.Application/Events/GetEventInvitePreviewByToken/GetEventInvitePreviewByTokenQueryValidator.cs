using FluentValidation;

namespace EventCo.Application.Events.GetEventInvitePreviewByToken;

public sealed class GetEventInvitePreviewByTokenQueryValidator : AbstractValidator<GetEventInvitePreviewByTokenQuery>
{
    public GetEventInvitePreviewByTokenQueryValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
