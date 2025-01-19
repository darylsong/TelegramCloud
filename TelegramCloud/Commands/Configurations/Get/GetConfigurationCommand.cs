using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.Configurations.Get;

public class GetConfigurationCommand : Command
{
    public GetConfigurationCommand() : base("get", "Get Telegram configuration")
    {
        var commandHandler = CommandHandler.Create<IDatabaseContext>(databaseContext =>
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

        Handler = commandHandler;
    }
}