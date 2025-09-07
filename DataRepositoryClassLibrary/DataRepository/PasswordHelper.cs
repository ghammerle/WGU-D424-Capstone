
using System.Security.Cryptography;


namespace C424Assessment.DataRepository
{

    public class PasswordHelper
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            // Generate random salt (16 bytes)
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);

            // Derive hash using PBKDF2 (SHA256, 100k iterations)
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32); // 256-bit hash

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(hashBytes) == storedHash;
        }
    }
}
