using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramCloud.Infrastructure;

public interface ITelegramBot
{
    Task GetFile(string telegramFileId, MemoryStream memoryStream);
    Task<string> SendFile(Stream fileStream);
}

public class TelegramBot(ITelegramConfigurationContext context) : ITelegramBot
{
    public async Task GetFile(string telegramFileId, MemoryStream memoryStream)
    {
        var (token, _) = context.GetRequiredConfiguration();

        var client = new TelegramBotClient(token);

        var fileInfo = await client.GetFile(telegramFileId);

        if (fileInfo.FilePath is null)
        {
            throw new FileNotFoundException($"Telegram file with {telegramFileId} not found.");
        }

        await client.DownloadFile(fileInfo.FilePath, memoryStream);
    }

    public async Task<string> SendFile(Stream fileStream)
    {
        var (token, chatId) = context.GetRequiredConfiguration();

        var client = new TelegramBotClient(token);
        
        var message = await client.SendDocument(
            new ChatId(chatId),
            InputFile.FromStream(fileStream));

        return message.Document!.FileId;
    }
}