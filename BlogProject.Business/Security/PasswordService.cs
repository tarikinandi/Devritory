using System.Security.Cryptography;
using System.Text;
using BlogProject.Entity;
using Microsoft.AspNetCore.Identity;

namespace BlogProject.Business.Security
{
    public enum PasswordCheck
    {
        Failed,
        Success,
        /// <summary>Password is right but the stored value is legacy plain text or an outdated hash.</summary>
        SuccessRehashNeeded
    }

    /// <summary>
    /// Wraps PasswordHasher&lt;User&gt; and knows how to read the legacy plain-text rows
    /// that existed before password hashing was introduced.
    /// </summary>
    public class PasswordService
    {
        private readonly IPasswordHasher<User> _hasher;

        public PasswordService(IPasswordHasher<User> hasher) => _hasher = hasher;

        public string Hash(User user, string password) => _hasher.HashPassword(user, password);

        /// <summary>
        /// ASP.NET Core Identity hashes (v2 and v3) are base64 and start with format marker 0x00 or 0x01,
        /// which is "AAAA" / "AQAA" in base64. Anything else is treated as legacy plain text.
        /// </summary>
        public static bool LooksHashed(string? stored)
        {
            if (string.IsNullOrEmpty(stored) || stored.Length < 40) return false;
            if (!stored.StartsWith("AQAAAA", StringComparison.Ordinal) &&
                !stored.StartsWith("AAAA", StringComparison.Ordinal)) return false;
            Span<byte> buffer = new byte[stored.Length];
            return Convert.TryFromBase64String(stored, buffer, out _);
        }

        public PasswordCheck Verify(User user, string password)
        {
            var stored = user.Password;
            if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(password)) return PasswordCheck.Failed;

            if (LooksHashed(stored))
            {
                // A hashed value must never be accepted as a plain-text password: someone who read the
                // hash from the database would otherwise be able to log in by submitting the hash itself.
                var result = _hasher.VerifyHashedPassword(user, stored, password);
                return result switch
                {
                    PasswordVerificationResult.Success => PasswordCheck.Success,
                    PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SuccessRehashNeeded,
                    _ => PasswordCheck.Failed
                };
            }

            var a = Encoding.UTF8.GetBytes(stored);
            var b = Encoding.UTF8.GetBytes(password);
            return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b)
                ? PasswordCheck.SuccessRehashNeeded
                : PasswordCheck.Failed;
        }
    }
}
