using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FTECH_THUONGMAIDIENTU.Infrastructure
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    var hash = deriveBytes.GetBytes(HashSize);
                    return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
                }
            }
        }

        public static bool Verify(string password, string storedValue)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedValue))
            {
                return false;
            }

            if (storedValue.StartsWith("PBKDF2$", StringComparison.OrdinalIgnoreCase))
            {
                var parts = storedValue.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
                {
                    return false;
                }

                try
                {
                    var salt = Convert.FromBase64String(parts[2]);
                    var expectedHash = Convert.FromBase64String(parts[3]);

                    using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
                    {
                        var actualHash = deriveBytes.GetBytes(expectedHash.Length);
                        return actualHash.SequenceEqual(expectedHash);
                    }
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            return string.Equals(password, storedValue, StringComparison.Ordinal);
        }
    }
}