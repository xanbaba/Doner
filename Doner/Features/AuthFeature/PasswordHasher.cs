using System.Security.Cryptography;
using System.Text;

namespace Doner.Features.AuthFeature;

public static class PasswordHasher
{
    public static byte[] HashPassword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(32);
        return HashPassword(password, salt);
    }
    
    public static byte[] HashPassword(string password, byte[] salt)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combined = new byte[passwordBytes.Length + salt.Length];

        Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
        Buffer.BlockCopy(salt, 0, combined, passwordBytes.Length, salt.Length);

        var hashedBytes = sha256.ComputeHash(combined);
        return hashedBytes;
    }

    public static bool ConstantTimeCompare(byte[] hash1, byte[] hash2)
    {
        if (hash1.Length != hash2.Length) return false; // Early return if lengths differ

        int result = 0; // Accumulator variable
        for (int i = 0; i < hash1.Length; i++)
        {
            result |= hash1[i] ^ hash2[i]; // XOR each byte and accumulate result
        }

        return result == 0; // If result is 0, hashes are identical
    }

}