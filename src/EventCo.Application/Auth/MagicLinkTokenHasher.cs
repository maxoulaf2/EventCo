using System.Security.Cryptography;
using System.Text;

namespace EventCo.Application.Auth;

internal static class MagicLinkTokenHasher
{
    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
