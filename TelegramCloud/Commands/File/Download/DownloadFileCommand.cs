using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Commands.File.Shared;
using TelegramCloud.Infrastructure;
using TelegramCloud.Services;

namespace TelegramCloud.Commands.File.Download;

public class DownloadFileCommand : Command
{
    public DownloadFileCommand() : base("download", "Download file")
    {
        AddArgument(new FileIdArgument());

        AddOption(new OutputFileNameOption());

        Handler = CommandHandler
            .Create<Guid, string?, IFileEncryptionService, ITelegramConfigurationContext, IFilesContext>(async (
                fileId,
                outputFileName,
                encryptionService,
                telegramConfigurationContext,
                filesContext) =>
            {
                var (token, chatId) = telegramConfigurationContext.GetRequiredConfiguration();
                var telegramBot = new TelegramBot(token, chatId);

                var file = await filesContext.GetFile(fileId);

                if (file is null)
                {
                    Console.WriteLine($"File with ID {fileId} not found.");
                    return;
                }

                Console.WriteLine($"Downloading file {file.Name} with size {file.Size} bytes.");

                var fileChunks = filesContext
                    .GetFileChunks(fileId)
                    .OrderBy(x => x.ChunkNumber);

                var outputPath = outputFileName is null
                    ? $"{Environment.CurrentDirectory}/{file.Name}"
                    : $"{Environment.CurrentDirectory}/{outputFileName}";

                try
                {
                    var encryptedData = new MemoryStream();
                    await foreach (var fileChunk in fileChunks)
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
                catch (Exception)
                {
                    if (System.IO.File.Exists(outputPath))
                    {
                        System.IO.File.Delete(outputPath);
                    }

                    throw;
                }
            });
    }
}