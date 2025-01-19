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
        
        Handler = CommandHandler.Create<string?, int?, IDatabaseContext>(async (token, chatId, databaseContext) =>
        {
            if (token is not null)
            {
                await databaseContext.SetTelegramBotTokenConfig(token);
                Console.WriteLine("Successfully set Telegram bot API access token");
            }

            if (chatId is not null)
            {
                await databaseContext.SetTelegramBotChatIdConfig(chatId.Value);
                Console.WriteLine("Successfully set Telegram chat ID");
            }
        });
    }
}