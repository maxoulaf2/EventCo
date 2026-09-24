using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventCo.Infrastructure.Storage;

// Stockage S3-compatible — Neon Object Storage en production (cf. CLAUDE.md § Déploiement). Le bucket
// est en accès public_read : les fichiers sont lus directement par le navigateur via GetPublicUrl, sans
// repasser par l'API ; seules les écritures/suppressions sont authentifiées.
internal sealed class S3FileStorage : IFileStorage, IDisposable
{
    // Clé aléatoire à chaque upload (cf. UpdateAvatarCommandHandler) : le contenu d'une URL ne change
    // jamais, le navigateur peut le garder en cache indéfiniment.
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";

    private readonly S3StorageOptions _options;
    private readonly AmazonS3Client _client;
    private readonly string _publicBaseUrl;
    private readonly ILogger<S3FileStorage> _logger;

    public S3FileStorage(IOptions<StorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _options = options.Value.S3;
        _logger = logger;

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

        _publicBaseUrl = string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? $"{_options.ServiceUrl.TrimEnd('/')}/{_options.BucketName}"
            : _options.PublicBaseUrl.TrimEnd('/');
    }

    public async Task UploadAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content, writable: false);

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            // Signature du corps complet plutôt qu'en flux "aws-chunked", même raison que pour les
            // checksums ci-dessus (fichiers de quelques dizaines de Ko, déjà en mémoire).
            UseChunkEncoding = false,
        };
        request.Headers.CacheControl = ImmutableCacheControl;

        await _client.PutObjectAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
        }
        catch (AmazonServiceException exception)
        {
            _logger.LogWarning(exception, "Suppression du fichier {Key} impossible (fichier orphelin laissé dans le bucket).", key);
        }
    }

    public string GetPublicUrl(string key) => $"{_publicBaseUrl}/{key}";

    public void Dispose() => _client.Dispose();
}
