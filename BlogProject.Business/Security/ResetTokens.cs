using System.Security.Cryptography;
using System.Text;

namespace BlogProject.Business.Security
{
    /// <summary>Password reset tokens: random for the user, only the SHA-256 is stored.</summary>
    public static class ResetTokens
    {
        public static string Generate() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        public static string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        public static bool Matches(string? storedHash, string? token)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(token)) return false;
            var a = Encoding.UTF8.GetBytes(storedHash);
            var b = Encoding.UTF8.GetBytes(Hash(token));
            return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}
