using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Commands.File.Shared;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.File.Delete;

public class DeleteFileCommand : Command
{
    public DeleteFileCommand() : base("delete", "Delete file")
    {
        AddArgument(new FileIdArgument());

        Handler = CommandHandler
            .Create<Guid, IFilesContext>(async (fileId, filesContext) =>
            {
                await filesContext.DeleteFile(fileId);
                Console.WriteLine("File successfully deleted");
            });
    }
}