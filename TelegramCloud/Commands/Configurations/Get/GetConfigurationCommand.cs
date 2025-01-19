using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.Configurations.Get;

public class GetConfigurationCommand : Command
{
    public GetConfigurationCommand() : base("get", "Get Telegram configuration")
    {
        Handler = CommandHandler.Create<ITelegramConfigurationContext>(telegramConfigurationContext =>
        {
            var config = telegramConfigurationContext.GetConfiguration();

            if (config is null)
            {
                Console.WriteLine("Telegram bot configuration has not been set.");
            }
            else
            {
                Console.WriteLine("API token: " + (config.Token ?? "Not set"));
                Console.WriteLine("Chat ID: " + (config.ChatId?.ToString() ?? "Not set"));
            }
        });
    }
}