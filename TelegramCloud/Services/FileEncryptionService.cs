using System.Security.Cryptography;

namespace TelegramCloud.Services;

public interface IFileEncryptionService
{
    (byte[] encryptedData, byte[] key, byte[] iv) Encrypt(byte[] data);
    byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv);
}

public class FileEncryptionService : IFileEncryptionService
{
    private const int KeySizeInBits = 256;

    public (byte[] encryptedData, byte[] key, byte[] iv) Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySizeInBits;
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
        aes.KeySize = KeySizeInBits;
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