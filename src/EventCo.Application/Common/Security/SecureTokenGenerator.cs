using System.Security.Cryptography;

namespace EventCo.Application.Common.Security;

internal static class SecureTokenGenerator
{
    public static string GenerateUrlSafeToken(int byteLength = 32) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
