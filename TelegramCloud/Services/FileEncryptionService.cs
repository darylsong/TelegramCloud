using System.Security.Cryptography;

namespace TelegramCloud.Services;

public class FileEncryptionService
{
    private const int KeySize = 32; // 256 bits
    private const int IvSize = 16;  // 128 bits

    public (byte[] encryptedData, byte[] key, byte[] iv) Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize * 8; // Convert bytes to bits
        aes.GenerateKey();
        aes.GenerateIV();

        using var msEncrypt = new MemoryStream();
        using var encryptor = aes.CreateEncryptor();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        
        csEncrypt.Write(data, 0, data.Length);
        csEncrypt.FlushFinalBlock();

        return (msEncrypt.ToArray(), aes.Key, aes.IV);
    }

    public byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize * 8;
        aes.Key = key;
        aes.IV = iv;

        using var msDecrypt = new MemoryStream();
        using var decryptor = aes.CreateDecryptor();
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);
        
        csDecrypt.Write(encryptedData, 0, encryptedData.Length);
        csDecrypt.FlushFinalBlock();

        return msDecrypt.ToArray();
    }
} 