using System.CommandLine;

namespace TelegramCloud.Commands.Configurations.Set;

public class TokenOption : Option<string?>
{
    public TokenOption() : base(
        name: "--token",
        description: "Set your Telegram bot API access token"
    )
    {
        AddAlias("-t");
    }
}