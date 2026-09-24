namespace EventCo.Application.Common.Interfaces;

// Stockage de fichiers publics (avatars, images d'événement...) : S3-compatible en production (Neon
// Object Storage, un bucket par FileStorageBucket), disque local en dev/tests quand aucun bucket n'est
// configuré (cf. Infrastructure/Storage).
public interface IFileStorage
{
    Task UploadAsync(FileStorageBucket bucket, string key, byte[] content, string contentType, CancellationToken cancellationToken);

    // Best effort : un échec est journalisé mais ne remonte pas. Appelé pour nettoyer un fichier devenu
    // inutile (ancien avatar), dont la présence résiduelle ne casse rien, alors que faire échouer la
    // requête laisserait croire à l'utilisateur que son changement n'a pas été pris en compte.
    Task DeleteAsync(FileStorageBucket bucket, string key, CancellationToken cancellationToken);

    string GetPublicUrl(FileStorageBucket bucket, string key);
}
