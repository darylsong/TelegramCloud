using System.CommandLine;

namespace TelegramCloud.Commands.File.Upload;

public class FilePathArgument : Argument<string>
{
    public FilePathArgument() : base(
        name: "filePath",
        description: "Path to file to be uploaded"
    )
    {
    }
}