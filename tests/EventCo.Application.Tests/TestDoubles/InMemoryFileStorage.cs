using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.TestDoubles;

public sealed class InMemoryFileStorage : IFileStorage
{
    public const string PublicBaseUrl = "https://storage.test/bucket";

    public Dictionary<string, StoredFile> Files { get; } = [];

    public Task UploadAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        Files[key] = new StoredFile(content, contentType);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        Files.Remove(key);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => $"{PublicBaseUrl}/{key}";
}

public sealed record StoredFile(byte[] Content, string ContentType);
