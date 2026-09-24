using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventCo.Infrastructure.Storage;

// Fallback quand aucun bucket S3 n'est configuré (Storage:S3:ServiceUrl vide), comme LoggingEmailSender
// pour l'email : fichiers écrits sur le disque de l'API et servis par elle sous RequestPath (cf.
// Program.cs), en same-origin. Utile en dev et pour les tests Api — pas en production sur Render, dont
// le disque est effacé à chaque déploiement. Chaque bucket correspond à un sous-dossier de RootDirectory.
public sealed class LocalFileStorage : IFileStorage
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment environment, ILogger<LocalFileStorage> logger)
    {
        _options = options.Value.Local;
        _logger = logger;
        RootDirectory = Path.GetFullPath(Path.Combine(environment.ContentRootPath, _options.RootPath));
    }

    public string RootDirectory { get; }

    public string RequestPath => _options.RequestPath;

    public async Task UploadAsync(FileStorageBucket bucket, string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        var path = ResolvePath(bucket, key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
    }

    public Task DeleteAsync(FileStorageBucket bucket, string key, CancellationToken cancellationToken)
    {
        try
        {
            File.Delete(ResolvePath(bucket, key));
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Suppression du fichier {Key} impossible (fichier orphelin laissé sur le disque).", key);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(FileStorageBucket bucket, string key) =>
        $"{_options.RequestPath.TrimEnd('/')}/{DirectoryName(bucket)}/{key}";

    private static string DirectoryName(FileStorageBucket bucket) => bucket switch
    {
        FileStorageBucket.Avatars => "avatars",
        FileStorageBucket.EventImages => "event-images",
        _ => throw new ArgumentOutOfRangeException(nameof(bucket), bucket, null),
    };

    // Les clés sont générées côté serveur, mais une clé qui sortirait du dossier du bucket (../) écrirait
    // n'importe où sur le disque : refusée par principe.
    private string ResolvePath(FileStorageBucket bucket, string key)
    {
        var bucketDirectory = Path.Combine(RootDirectory, DirectoryName(bucket));
        var path = Path.GetFullPath(Path.Combine(bucketDirectory, key));
        if (!path.StartsWith(bucketDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException($"Clé de stockage invalide : {key}.");

        return path;
    }
}
