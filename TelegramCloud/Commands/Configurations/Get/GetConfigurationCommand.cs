using System.CommandLine;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.Configurations.Get;

public class GetConfigurationCommand : Command
{
    private readonly DatabaseContext _dbContext = new();
    
    public GetConfigurationCommand() : base("get", "Get Telegram configuration")
    {
        this.SetHandler(_ =>
        {
            var config = _dbContext.GetTelegramBotConfig();
        
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