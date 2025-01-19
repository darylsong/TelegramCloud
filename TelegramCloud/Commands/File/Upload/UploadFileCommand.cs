using System.CommandLine;
using TelegramCloud.Infrastructure;
using TelegramCloud.Models;
using TelegramCloud.Services;

namespace TelegramCloud.Commands.File.Upload;

public class UploadFileCommand : Command
{
    private readonly FilesContext _dbContext = new();
    private const int ChunkSize = 20 * 1024 * 1024;
    
    public UploadFileCommand() : base("upload", "Upload file")
    {
        var filePathArgument = new FilePathArgument();
        AddArgument(filePathArgument);
        
        this.SetHandler(async (filePathArgumentValue) =>
        {
            var config = _dbContext.GetRequiredTelegramBotConfig();
            var telegramBot = new TelegramBot(config.Token!, config.ChatId!.Value);
            var encryptionService = new FileEncryptionService();
        
            // Read entire file into memory and encrypt it
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePathArgumentValue);
            var (encryptedData, encryptionKey, encryptionIv) = encryptionService.Encrypt(fileBytes);
        
            var totalFileSize = fileBytes.Length;
            var totalUploadedSize = 0L;
            var uploadedFileChunks = new List<UploadedFileChunkDto>();
        
            Console.WriteLine($"Uploading file {filePathArgumentValue} with size {totalFileSize} bytes.");

            try
            {
                for (int offset = 0; offset < encryptedData.Length; offset += ChunkSize)
                {
                    var chunkSize = Math.Min(ChunkSize, encryptedData.Length - offset);
                    var chunk = new byte[chunkSize];
                    Array.Copy(encryptedData, offset, chunk, 0, chunkSize);
                    
                    var chunkNumber = (offset / ChunkSize) + 1;
                    using var chunkStream = new MemoryStream(chunk);
                    var chunkLength = chunkStream.Length;
                    var telegramFileId = await telegramBot.SendFile(chunkStream);
                    uploadedFileChunks.Add(new UploadedFileChunkDto(telegramFileId, chunkNumber, chunkLength));
                    totalUploadedSize += chunkLength;

                    Console.WriteLine($"Upload progress: {(double)totalUploadedSize / encryptedData.Length * 100}%");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                throw;
            }
        
            var fileGuid = await _dbContext.InsertFile(
                Path.GetFileName(filePathArgumentValue), 
                totalFileSize,
                Convert.ToBase64String(encryptionKey),
                Convert.ToBase64String(encryptionIv));

            foreach (var fileChunk in uploadedFileChunks)
            {
                await _dbContext.InsertFileChunk(fileGuid, fileChunk.ChunkNumber, fileChunk.TelegramFileId, fileChunk.ChunkLength);
            }
        
            Console.WriteLine("File successfully uploaded.");
        }, filePathArgument);
    }
}