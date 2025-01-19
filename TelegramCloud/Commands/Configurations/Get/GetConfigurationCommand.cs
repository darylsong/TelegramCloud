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
            var (token, chatId) = telegramConfigurationContext.GetConfiguration();

            Console.WriteLine("API token: " + (token ?? "Not set"));
            Console.WriteLine("Chat ID: " + (chatId?.ToString() ?? "Not set"));
        });
    }
}