using System.CommandLine;
using TelegramCloud.Infrastructure;
using TelegramCloud.Models;

namespace TelegramCloud.Commands.File.Upload;

public class UploadFileCommand : Command
{
    private readonly DatabaseContext _dbContext = new();
    private const int ChunkSize = 20 * 1024 * 1024;
    
    public UploadFileCommand() : base("upload", "Upload file")
    {
        var filePathArgument = new FilePathArgument();
        AddArgument(filePathArgument);
        
        this.SetHandler(async (filePathArgumentValue) =>
        {
            var config = _dbContext.GetRequiredTelegramBotConfig();
            var telegramBot = new TelegramBot(config.Token!, config.ChatId!.Value);
        
            await using var reader = new FileStream(filePathArgumentValue, FileMode.Open);

            var totalFileSize = reader.Length;
            var totalUploadedSize = 0L;
            var uploadedFileChunks = new List<UploadedFileChunkDto>();
        
            Console.WriteLine($"Uploading file {filePathArgumentValue} with size {totalFileSize} bytes.");

            try
            {
                var buffer = new byte[ChunkSize];
                int bytesRead;
                var chunkNumber = 0;

                while ((bytesRead = await reader.ReadAsync(buffer.AsMemory(0, ChunkSize))) > 0)
                {
                    chunkNumber++;
                    using var chunkStream = new MemoryStream(buffer, 0, bytesRead);
                    var chunkLength = chunkStream.Length;
                    var telegramFileId = await telegramBot.SendFile(chunkStream);
                    uploadedFileChunks.Add(new UploadedFileChunkDto(telegramFileId, chunkNumber, chunkLength));
                    totalUploadedSize += chunkLength;

                    Console.WriteLine($"Upload progress: {(double)totalUploadedSize / totalFileSize * 100}%");
                }
             
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");

                throw;
            }
        
            var fileGuid = await _dbContext.InsertFile(Path.GetFileName(filePathArgumentValue), totalFileSize);

            foreach (var fileChunk in uploadedFileChunks)
            {
                await _dbContext.InsertFileChunk(fileGuid, fileChunk.ChunkNumber, fileChunk.TelegramFileId, fileChunk.ChunkLength);
            }
        
            Console.WriteLine("File successfully uploaded.");
        }, filePathArgument);
    }
}