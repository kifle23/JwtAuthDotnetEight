/*
  This implementation of password hashing uses the PBKDF2 algorithm (Rfc2898DeriveBytes) 
  as recommended by widely accepted security best practices. 
  The code is adapted based on best practices and guidelines provided by sources like:
  - OWASP Password Storage Cheat Sheet (https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
  - Microsoft Documentation on Cryptography (https://learn.microsoft.com/en-us/dotnet/standard/security/cryptography-model)
  
  Disclaimer: This is not original work, but rather follows established methods to ensure password security. 
*/
using System.Security.Cryptography;

namespace JwtAuthDotnetEight.Utilities
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000; // Recommended number of iterations

        public static string HashPassword(string password)
        {
            // Generate a salt
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Generate the hash using PBKDF2
            var hash = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = hash.GetBytes(KeySize);

            // Return the salt and hash as a single string
            var result = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, result, 0, SaltSize);
            Array.Copy(key, 0, result, SaltSize, KeySize);

            return Convert.ToBase64String(result);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Extract the bytes from the hash
            var hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract the salt from the hash
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Hash the input password with the extracted salt
            var hash = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = hash.GetBytes(KeySize);

            // Extract the stored key from the hash
            byte[] storedKey = new byte[KeySize];
            Array.Copy(hashBytes, SaltSize, storedKey, 0, KeySize);

            // Compare the hashed password with the stored hash
            return storedKey.SequenceEqual(key);
        }
    }
}
