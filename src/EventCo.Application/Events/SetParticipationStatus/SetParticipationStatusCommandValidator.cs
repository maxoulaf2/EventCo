using EventCo.Domain.Events;
using FluentValidation;

namespace EventCo.Application.Events.SetParticipationStatus;

public sealed class SetParticipationStatusCommandValidator : AbstractValidator<SetParticipationStatusCommand>
{
    public SetParticipationStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<ParticipationStatus>(status, out _))
            .WithMessage("Le statut de participation doit être l'une des valeurs suivantes : Attending, NotAttending, Unknown.");
    }
}
