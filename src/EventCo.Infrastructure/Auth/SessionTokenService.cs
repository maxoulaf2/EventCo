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

        var payload = new SessionTokenData(userId, email, expiresAt);
        var payloadSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signatureSegment = Base64UrlEncode(SignPayload(payloadSegment, sessionOptions.Secret));

        return new SessionToken($"{payloadSegment}.{signatureSegment}", expiresAt);
    }

    public SessionTokenData? ValidateSessionToken(string token, DateTime now)
    {
        var segments = token.Split('.');
        if (segments.Length != 2)
        {
            return null;
        }

        var (payloadSegment, signatureSegment) = (segments[0], segments[1]);

        byte[] providedSignature;
        try
        {
            providedSignature = Base64UrlDecode(signatureSegment);
        }
        catch (FormatException)
        {
            return null;
        }

        var expectedSignature = SignPayload(payloadSegment, options.Value.Secret);
        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
        {
            return null;
        }

        SessionTokenData? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SessionTokenData>(Base64UrlDecode(payloadSegment));
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }

        return payload is not null && payload.ExpiresAt > now ? payload : null;
    }

    private static byte[] SignPayload(string payloadSegment, string secret) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payloadSegment));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            _ => string.Empty,
        };

        return Convert.FromBase64String(padded);
    }
}
