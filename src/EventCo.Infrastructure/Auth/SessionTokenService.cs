using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace EventCo.Infrastructure.Auth;

// Token de session auto-porté (payload + signature HMAC), même famille de mécanisme que le hash des
// magic links, plutôt qu'une dépendance JWT supplémentaire (cf. cadrage §2.1 : "JWT ou identifiant de session").
internal sealed class SessionTokenService(IOptions<SessionOptions> options) : ISessionTokenService
{
    public SessionToken CreateSessionToken(Guid userId, string email, DateTime now)
    {
        var sessionOptions = options.Value;
        var expiresAt = now.AddDays(sessionOptions.ExpiryDays);

        var payload = new SessionTokenPayload(userId, email, expiresAt);
        var payloadSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signatureSegment = Base64UrlEncode(SignPayload(payloadSegment, sessionOptions.Secret));

        return new SessionToken($"{payloadSegment}.{signatureSegment}", expiresAt);
    }

    private static byte[] SignPayload(string payloadSegment, string secret) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payloadSegment));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private sealed record SessionTokenPayload(Guid UserId, string Email, DateTime ExpiresAt);
}
