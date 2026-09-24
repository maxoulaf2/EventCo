using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventCo.Infrastructure.Storage;

// Stockage S3-compatible — Neon Object Storage en production (cf. CLAUDE.md § Déploiement), un bucket
// par FileStorageBucket. Les buckets sont en accès public_read : les fichiers sont lus directement par le
// navigateur via GetPublicUrl, sans repasser par l'API ; seules les écritures/suppressions sont
// authentifiées.
internal sealed class S3FileStorage : IFileStorage, IDisposable
{
    // Clé aléatoire à chaque upload (cf. UpdateAvatarCommandHandler, UpdateEventImageCommandHandler) : le
    // contenu d'une URL ne change jamais, le navigateur peut le garder en cache indéfiniment.
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";

    private readonly S3StorageOptions _options;
    private readonly AmazonS3Client _client;
    private readonly Dictionary<FileStorageBucket, string> _publicBaseUrls;
    private readonly ILogger<S3FileStorage> _logger;

    public S3FileStorage(IOptions<StorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _options = options.Value.S3;
        _logger = logger;

        // Échec au démarrage (IFileStorage est résolu par Program.cs) plutôt qu'au premier upload : un
        // bucket oublié dans la configuration ne passe pas inaperçu.
        var missingBuckets = Enum.GetValues<FileStorageBucket>()
            .Where(bucket => string.IsNullOrWhiteSpace(_options.Buckets.For(bucket).Name))
            .ToList();
        if (missingBuckets.Count > 0)
            throw new InvalidOperationException(
                $"Stockage S3 configuré sans nom de bucket pour : {string.Join(", ", missingBuckets)} (Storage:S3:Buckets:<bucket>:Name).");

        _client = new AmazonS3Client(
            new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey),
            new AmazonS3Config
            {
                ServiceURL = _options.ServiceUrl,
                AuthenticationRegion = _options.Region,
                // Neon ne supporte que l'adressage path-style (https://endpoint/bucket/key).
                ForcePathStyle = true,
                // Le SDK v4 calcule par défaut des checksums "flexibles" (CRC32, en-têtes de fin de
                // requête), mal supportés par les stockages S3-compatibles non-AWS : limités au strict
                // nécessaire.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            });

        _publicBaseUrls = Enum.GetValues<FileStorageBucket>().ToDictionary(bucket => bucket, bucket =>
        {
            var bucketOptions = _options.Buckets.For(bucket);
            return string.IsNullOrWhiteSpace(bucketOptions.PublicBaseUrl)
                ? $"{_options.ServiceUrl.TrimEnd('/')}/{bucketOptions.Name}"
                : bucketOptions.PublicBaseUrl.TrimEnd('/');
        });
    }

    public async Task UploadAsync(FileStorageBucket bucket, string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content, writable: false);

        var request = new PutObjectRequest
        {
            BucketName = BucketName(bucket),
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            // Signature du corps complet plutôt qu'en flux "aws-chunked", même raison que pour les
            // checksums ci-dessus (fichiers de quelques centaines de Ko au plus, déjà en mémoire).
            UseChunkEncoding = false,
        };
        request.Headers.CacheControl = ImmutableCacheControl;

        await _client.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(FileStorageBucket bucket, string key, CancellationToken cancellationToken)
    {
        try
        {
            await _client.DeleteObjectAsync(BucketName(bucket), key, cancellationToken);
        }
        catch (AmazonServiceException exception)
        {
            _logger.LogWarning(exception, "Suppression du fichier {Key} impossible (fichier orphelin laissé dans le bucket {Bucket}).", key, bucket);
        }
    }

    public string GetPublicUrl(FileStorageBucket bucket, string key) => $"{_publicBaseUrls[bucket]}/{key}";

    private string BucketName(FileStorageBucket bucket) => _options.Buckets.For(bucket).Name;

    public void Dispose() => _client.Dispose();
}
