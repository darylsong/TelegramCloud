namespace TelegramCloud.Models;

public record UploadedFileChunkDto(string TelegramFileId, int ChunkNumber, long ChunkLength);