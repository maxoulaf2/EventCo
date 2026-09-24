using EventCo.Application.Common.Images;
using FluentValidation;

namespace EventCo.Application.Auth.UpdateAvatar;

public sealed class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
{
    // Le frontend redimensionne l'image avant envoi (quelques dizaines de Ko) : cette limite ne vise que
    // les appels directs à l'API.
    public const int MaxContentBytes = 2 * 1024 * 1024;

    public UpdateAvatarCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Aucune image n'a été envoyée.");

        RuleFor(x => x.Content)
            .Must(content => content.Length <= MaxContentBytes)
            .WithMessage("L'image ne doit pas dépasser 2 Mo.");

        RuleFor(x => x.Content)
            .Must(content => ImageFormat.Detect(content) is not null)
            .When(x => x.Content.Length > 0)
            .WithMessage("Format d'image non supporté (JPEG, PNG ou WebP attendu).");
    }
}
