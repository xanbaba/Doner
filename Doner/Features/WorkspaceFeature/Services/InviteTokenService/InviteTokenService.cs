using System.Security.Cryptography;
using System.Text;

namespace Doner.Features.WorkspaceFeature.Services.InviteLinkService;

public class InviteTokenService : IInviteTokenService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public InviteTokenService(IConfiguration config)
    {
        var secret = config["InviteEncryptionKey"] ?? throw new Exception("Missing InviteEncryptionKey in configuration");
        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
        _iv = _key.Take(16).ToArray(); // use first 16 bytes as IV
    }

    public string GenerateToken(Guid workspaceId, Guid userId)
    {
        var plainText = $"{workspaceId}|{userId}";
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""); // URL safe
    }

    public (Guid workspaceId, Guid userId)? DecryptToken(string encrypted)
    {
        try
        {
            var base64 = encrypted
                .Replace("-", "+")
                .Replace("_", "/");

            // Add padding back if needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var encryptedBytes = Convert.FromBase64String(base64);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            var parts = decryptedText.Split('|');
            if (parts.Length != 2) return null;

            return (Guid.Parse(parts[0]), Guid.Parse(parts[1]));
        }
        catch
        {
            return null; // Invalid or tampered data
        }
    }
}