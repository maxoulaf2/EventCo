using System.Security.Cryptography;
using System.Text;

namespace EventCo.Application.Auth;

internal static class MagicLinkTokenHasher
{
    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    public static bool Matches(string rawToken, string tokenHash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Hash(rawToken)),
            Encoding.UTF8.GetBytes(tokenHash));
}
