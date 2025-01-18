namespace TelegramCloud.Models;

public record FileDto(Guid Id, string Name, long Size, string EncryptionKey, string EncryptionIv);