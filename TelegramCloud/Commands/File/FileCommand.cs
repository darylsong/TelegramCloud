using System.CommandLine;
using TelegramCloud.Commands.File.Delete;
using TelegramCloud.Commands.File.Download;
using TelegramCloud.Commands.File.List;
using TelegramCloud.Commands.File.Upload;

namespace TelegramCloud.Commands.File;

public class FileCommand : Command
{
    public FileCommand() : base("file", "Manage files")
    {
        AddCommand(new ListFilesCommand());
        AddCommand(new DownloadFileCommand());
        AddCommand(new UploadFileCommand());
        AddCommand(new DeleteFileCommand());
    }
}