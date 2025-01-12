using System.CommandLine;

namespace TelegramCloud.Commands.Configurations.Set;

public class ChatIdOption : Option<int?>
{
    public ChatIdOption() : base(
        name: "--chat-id",
        description: "Set your Telegram chat ID"
    )
    {
        AddAlias("-c");
    }
}