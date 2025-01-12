using System.CommandLine;

namespace TelegramCloud.Commands.File.Shared;

public class FileIdArgument : Argument<Guid>
{
    public FileIdArgument() : base(
        name: "fileId",
        description: "File ID"
    )
    {
    }
}