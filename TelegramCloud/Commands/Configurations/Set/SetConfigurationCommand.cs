using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.Configurations.Set;

public class SetConfigurationCommand : Command
{
    public SetConfigurationCommand() : base("set", "Set Telegram configuration")
    {
        AddOption(new TokenOption());
        AddOption(new ChatIdOption());

        Handler = CommandHandler.Create<string?, int?, ITelegramConfigurationContext>(
            async (token, chatId, telegramConfigurationContext) =>
            {
                if (token is not null)
                {
                    await telegramConfigurationContext.SetBotToken(token);
                    Console.WriteLine("Successfully set Telegram bot API access token");
                }

                if (chatId is not null)
                {
                    await telegramConfigurationContext.SetChatId(chatId.Value);
                    Console.WriteLine("Successfully set Telegram chat ID");
                }
            });
    }
}