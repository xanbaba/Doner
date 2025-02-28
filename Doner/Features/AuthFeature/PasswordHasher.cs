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
}