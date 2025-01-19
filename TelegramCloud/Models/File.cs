namespace TelegramCloud.Models;

public record File(Guid Id, string Name, long Size, string EncryptionKey, string EncryptionIv);