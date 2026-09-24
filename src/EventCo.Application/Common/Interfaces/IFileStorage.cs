namespace EventCo.Application.Common.Interfaces;

// Stockage de fichiers publics (avatars...) : S3-compatible en production (Neon Object Storage),
// disque local en dev/tests quand aucun bucket n'est configuré (cf. Infrastructure/Storage).
public interface IFileStorage
{
    Task UploadAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken);

    // Best effort : un échec est journalisé mais ne remonte pas. Appelé pour nettoyer un fichier devenu
    // inutile (ancien avatar), dont la présence résiduelle ne casse rien, alors que faire échouer la
    // requête laisserait croire à l'utilisateur que son changement n'a pas été pris en compte.
    Task DeleteAsync(string key, CancellationToken cancellationToken);

    string GetPublicUrl(string key);
}
