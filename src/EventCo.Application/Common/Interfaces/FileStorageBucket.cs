namespace EventCo.Application.Common.Interfaces;

// Un bucket dédié par usage : cycle de vie, quotas et nettoyage indépendants (ex: vider les images
// d'événement sans toucher aux avatars).
public enum FileStorageBucket
{
    Avatars,
    EventImages,
}
