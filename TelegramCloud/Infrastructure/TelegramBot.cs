using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramCloud.Infrastructure;

public class TelegramBot(string token, int chatId)
{
    private readonly TelegramBotClient _client = new(token);
    private readonly ChatId _chatId = new(chatId);

    public async Task GetFile(string telegramFileId, MemoryStream memoryStream)
    {
        var fileInfo = await _client.GetFile(telegramFileId);

        if (fileInfo?.FilePath is null)
        {
            throw new FileNotFoundException($"Telegram file with {telegramFileId} not found.");
        }

        await _client.DownloadFile(fileInfo.FilePath, memoryStream);
    }

    public async Task<string> SendFile(Stream fileStream)
    {
        try
        {
            var message = await _client.SendDocument(
                _chatId,
                InputFile.FromStream(fileStream));
            
            return message.Document!.FileId;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error:", e);
            throw;
        }
    }
}