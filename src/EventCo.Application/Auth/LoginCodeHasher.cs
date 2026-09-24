using System.Security.Cryptography;
using System.Text;

namespace EventCo.Application.Auth;

internal static class LoginCodeHasher
{
    public static string Hash(string rawCode) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawCode)));

    public static bool Matches(string rawCode, string codeHash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Hash(rawCode)),
            Encoding.UTF8.GetBytes(codeHash));
}
