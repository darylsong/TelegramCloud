using System.CommandLine;
using TelegramCloud.Commands.Configurations.Get;
using TelegramCloud.Commands.Configurations.Set;

namespace TelegramCloud.Commands.Configurations;

public class ConfigurationCommand : Command
{
    public ConfigurationCommand() : base("configuration", "Manage Telegram configuration")
    {
        AddCommand(new GetConfigurationCommand());
        AddCommand(new SetConfigurationCommand());
    }
}