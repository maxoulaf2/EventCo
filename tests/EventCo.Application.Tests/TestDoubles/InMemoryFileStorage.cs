using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.TestDoubles;

public sealed class InMemoryFileStorage : IFileStorage
{
    public const string PublicBaseUrl = "https://storage.test";

    public Dictionary<StoredFileLocation, StoredFile> Files { get; } = [];

    public Task UploadAsync(FileStorageBucket bucket, string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        Files[new StoredFileLocation(bucket, key)] = new StoredFile(content, contentType);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FileStorageBucket bucket, string key, CancellationToken cancellationToken)
    {
        Files.Remove(new StoredFileLocation(bucket, key));
        return Task.CompletedTask;
    }

    public string GetPublicUrl(FileStorageBucket bucket, string key) => $"{PublicBaseUrl}/{bucket}/{key}";
}

public sealed record StoredFileLocation(FileStorageBucket Bucket, string Key);

public sealed record StoredFile(byte[] Content, string ContentType);
