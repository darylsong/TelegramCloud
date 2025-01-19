namespace TelegramCloud.Models;

public record UploadedFileChunk(string TelegramFileId, int ChunkNumber, long ChunkLength);