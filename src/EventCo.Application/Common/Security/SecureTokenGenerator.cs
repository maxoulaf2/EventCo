using System.Security.Cryptography;

namespace EventCo.Application.Common.Security;

internal static class SecureTokenGenerator
{
    public static string GenerateUrlSafeToken(int byteLength = 32) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    // Tirage uniforme (RandomNumberGenerator.GetInt32, pas de biais de modulo), complété par des zéros à gauche.
    public static string GenerateNumericCode(int digits = 6) =>
        RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, digits)).ToString($"D{digits}");
}
