using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Infrastructure;
using TelegramCloud.Models;
using TelegramCloud.Services;

namespace TelegramCloud.Commands.File.Upload;

public class UploadFileCommand : Command
{
    private const int ChunkSize = 20 * 1024 * 1024; // 20 megabytes

    public UploadFileCommand() : base("upload", "Upload file")
    {
        AddArgument(new FilePathArgument());
        
        Handler = CommandHandler
            .Create<string, IFileEncryptionService, ITelegramConfigurationContext, IFilesContext>(async (
                filePath,
                encryptionService,
                telegramConfigurationContext,
                filesContext) =>
            {
                var (token, chatId) = telegramConfigurationContext.GetRequiredConfiguration();
                var telegramBot = new TelegramBot(token, chatId);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var (encryptedData, encryptionKey, encryptionIv) = encryptionService.Encrypt(fileBytes);

                var totalFileSize = fileBytes.Length;
                var totalUploadedSize = 0L;
                var uploadedFileChunks = new List<UploadedFileChunkDto>();

                Console.WriteLine($"Uploading file {filePath} with size {totalFileSize} bytes.");

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

                        Console.WriteLine(
                            $"Upload progress: {(double)totalUploadedSize / encryptedData.Length * 100}%");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                    throw;
                }

                var fileGuid = await filesContext.InsertFile(
                    Path.GetFileName(filePath),
                    totalFileSize,
                    Convert.ToBase64String(encryptionKey),
                    Convert.ToBase64String(encryptionIv));

                foreach (var fileChunk in uploadedFileChunks)
                {
                    await filesContext.InsertFileChunk(fileGuid, fileChunk.ChunkNumber, fileChunk.TelegramFileId,
                        fileChunk.ChunkLength);
                }

                Console.WriteLine("File successfully uploaded.");
            });
    }
}