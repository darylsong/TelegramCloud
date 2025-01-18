using System.CommandLine;
using TelegramCloud.Commands.File.Shared;
using TelegramCloud.Infrastructure;
using TelegramCloud.Services;

namespace TelegramCloud.Commands.File.Download;

public class DownloadFileCommand : Command
{
    private readonly DatabaseContext _dbContext = new();
    
    public DownloadFileCommand() : base("download", "Download file")
    {
        var fileIdArgument = new FileIdArgument();
        AddArgument(fileIdArgument);
        
        var outputFileNameOption = new OutputFileNameOption();
        AddOption(outputFileNameOption);
        
        this.SetHandler(async (fileIdArgumentValue, outputFileNameOptionValue) =>
        {
            var config = _dbContext.GetRequiredTelegramBotConfig();
            var telegramBot = new TelegramBot(config.Token!, config.ChatId!.Value);
            var encryptionService = new FileEncryptionService();
            
            var file = _dbContext.GetFile(fileIdArgumentValue);

            if (file is null)
            {
                Console.WriteLine($"File with ID {fileIdArgumentValue} not found.");
                return;
            }

            Console.WriteLine($"Downloading file {file.Name} with size {file.Size} bytes.");

            var fileChunks = _dbContext
                .GetFileChunks(fileIdArgumentValue)
                .OrderBy(x => x.ChunkNumber);

            var outputPath = outputFileNameOptionValue is null
                ? $"{Environment.CurrentDirectory}/{fileIdArgumentValue}"
                : $"{Environment.CurrentDirectory}/{outputFileNameOptionValue}";

            try
            {
                // Download all chunks into a single encrypted data array
                var encryptedData = new MemoryStream();
                foreach (var fileChunk in fileChunks)
                {
                    using var chunkStream = new MemoryStream();
                    await telegramBot.GetFile(fileChunk.TelegramFileId, chunkStream);
                    var chunkData = chunkStream.ToArray();
                    await encryptedData.WriteAsync(chunkData);
                    
                    Console.WriteLine($"Download progress: {(double)encryptedData.Length / file.Size * 100}%");
                }

                var key = Convert.FromBase64String(file.EncryptionKey);
                var iv = Convert.FromBase64String(file.EncryptionIv);
                var decryptedData = encryptionService.Decrypt(encryptedData.ToArray(), key, iv);

                await System.IO.File.WriteAllBytesAsync(outputPath, decryptedData);

                Console.WriteLine("Successfully downloaded file.");
            }
            catch (Exception e)
            {
                if (System.IO.File.Exists(outputPath))
                {
                    System.IO.File.Delete(outputPath);
                }
                throw;
            }
        }, fileIdArgument, outputFileNameOption);
    }
}