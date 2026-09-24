using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Common.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public S3StorageOptions S3 { get; init; } = new();

    public LocalStorageOptions Local { get; init; } = new();
}

// Neon Object Storage (ou tout stockage S3-compatible). ServiceUrl vide = fallback disque local.
public sealed class S3StorageOptions
{
    // Endpoint de la branche Neon, ex: https://br-xxx.storage.c-1.eu-central-1.aws.neon.tech
    public string ServiceUrl { get; init; } = string.Empty;

    // Région AWS de la branche, sans le préfixe "aws-" de Neon (ex: eu-central-1).
    public string Region { get; init; } = string.Empty;

    // Une même clé d'accès pour tous les buckets (générée au niveau de la branche Neon).
    public string AccessKeyId { get; init; } = string.Empty;

    public string SecretAccessKey { get; init; } = string.Empty;

    public S3BucketsOptions Buckets { get; init; } = new();

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ServiceUrl);
}

public sealed class S3BucketsOptions
{
    public S3BucketOptions Avatars { get; init; } = new();

    public S3BucketOptions EventImages { get; init; } = new();

    public S3BucketOptions For(FileStorageBucket bucket) => bucket switch
    {
        FileStorageBucket.Avatars => Avatars,
        FileStorageBucket.EventImages => EventImages,
        _ => throw new ArgumentOutOfRangeException(nameof(bucket), bucket, null),
    };
}

public sealed class S3BucketOptions
{
    public string Name { get; init; } = string.Empty;

    // Base des URLs publiques (bucket public_read) : par défaut {ServiceUrl}/{Name}, en adressage
    // path-style (seul mode supporté par Neon).
    public string PublicBaseUrl { get; init; } = string.Empty;
}

public sealed class LocalStorageOptions
{
    // Relatif au content root de l'API si non absolu. Servi par l'API sous RequestPath, un sous-dossier
    // par bucket (cf. LocalFileStorage).
    public string RootPath { get; init; } = "uploads";

    public string RequestPath { get; init; } = "/uploads";
}
