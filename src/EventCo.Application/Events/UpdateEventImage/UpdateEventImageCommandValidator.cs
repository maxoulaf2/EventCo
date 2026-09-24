using EventCo.Application.Common.Images;
using FluentValidation;

namespace EventCo.Application.Events.UpdateEventImage;

public sealed class UpdateEventImageCommandValidator : AbstractValidator<UpdateEventImageCommand>
{
    // Le frontend redimensionne l'image avant envoi (quelques centaines de Ko) : cette limite ne vise que
    // les appels directs à l'API, ou un navigateur incapable de redimensionner (fichier envoyé tel quel).
    public const int MaxContentBytes = 5 * 1024 * 1024;

    public UpdateEventImageCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Aucune image n'a été envoyée.");

        RuleFor(x => x.Content)
            .Must(content => content.Length <= MaxContentBytes)
            .WithMessage("L'image ne doit pas dépasser 5 Mo.");

        RuleFor(x => x.Content)
            .Must(content => ImageFormat.Detect(content) is not null)
            .When(x => x.Content.Length > 0)
            .WithMessage("Format d'image non supporté (JPEG, PNG ou WebP attendu).");
    }
}
