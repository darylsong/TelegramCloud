using System.CommandLine;
using TelegramCloud.Commands.File.Shared;
using TelegramCloud.Infrastructure;

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
            
            var file = _dbContext.GetFileSize(fileIdArgumentValue);

            if (file is null)
            {
                Console.WriteLine($"File with ID {fileIdArgumentValue} not found.");

                return;
            }

            Console.WriteLine($"Downloading file {file.Name} with size {file.Size} bytes.");

            var fileChunks = _dbContext
                .GetAllFileChunks(fileIdArgumentValue)
                .OrderBy(x => x.ChunkNumber);

            var outputPath = outputFileNameOptionValue is null
                ? $"{Environment.CurrentDirectory}/{fileIdArgumentValue}"
                : $"{Environment.CurrentDirectory}/{outputFileNameOptionValue}";

            try
            {
                await using var fileStream = new FileStream(outputPath, FileMode.Create);

                foreach (var fileChunk in fileChunks)
                {
                    await telegramBot.GetFile(fileChunk.TelegramFileId, fileStream);

                    Console.WriteLine($"Download progress: {(double) fileStream.Length / file.Size * 100}%");
                }

                Console.WriteLine("Successfully downloaded file.");
            }
            catch (Exception e)
            {
                if (System.IO.File.Exists(outputPath))
                {
                    System.IO.File.Delete(outputPath);
                }
            }

        }, fileIdArgument, outputFileNameOption);
    }
}