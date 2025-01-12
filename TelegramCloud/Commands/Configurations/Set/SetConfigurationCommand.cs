using System.CommandLine;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.Configurations.Set;

public class SetConfigurationCommand : Command
{
    private readonly DatabaseContext _dbContext = new();

    public SetConfigurationCommand() : base("set", "Set Telegram configuration")
    {
        var tokenOption = new TokenOption();
        AddOption(tokenOption);

        var chatIdOption = new ChatIdOption();
        AddOption(chatIdOption);

        this.SetHandler(async (tokenOptionValue, chatIdOptionValue) =>
        {
            if (tokenOptionValue is not null)
            {
                await _dbContext.SetTelegramBotTokenConfig(tokenOptionValue);
                Console.WriteLine("Successfully set Telegram bot API access token");
            }

            if (chatIdOptionValue is not null)
            {
                await _dbContext.SetTelegramBotChatIdConfig(chatIdOptionValue.Value);
                Console.WriteLine("Successfully set Telegram chat ID");
            }
        }, tokenOption, chatIdOption);
    }
}