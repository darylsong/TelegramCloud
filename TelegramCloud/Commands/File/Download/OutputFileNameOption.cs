using System.CommandLine;

namespace TelegramCloud.Commands.File.Download;

public class OutputFileNameOption : Option<string?>
{
    public OutputFileNameOption() : base(
        name: "--output",
        description: "Output file name"
    )
    {
        AddAlias("-o");
    }
}