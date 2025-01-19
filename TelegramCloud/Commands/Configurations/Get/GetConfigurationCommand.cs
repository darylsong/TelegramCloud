using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.Configurations.Get;

public class GetConfigurationCommand : Command
{
    public GetConfigurationCommand() : base("get", "Get Telegram configuration")
    {
        Handler = CommandHandler.Create<IDatabaseContext>(databaseContext =>
        {
            var config = databaseContext.GetTelegramBotConfig();

            if (config is null)
            {
                Console.WriteLine("Telegram bot configuration has not been set.");
            }
            else
            {
                Console.WriteLine("API token: " + (config.Token ?? "Not set"));
                Console.WriteLine("Chat ID: " + (config.ChatId?.ToString() ?? "Not set"));
            }

            return Task.CompletedTask;
        });
    }
}