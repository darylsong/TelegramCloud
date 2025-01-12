using System.CommandLine;
using TelegramCloud.Commands.File.Shared;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.File.Delete;

public class DeleteFileCommand : Command
{
    private readonly DatabaseContext _dbContext = new();
    private const int ChunkSize = 20 * 1024 * 1024;
    
    public DeleteFileCommand() : base("delete", "Delete file")
    {
        var fileIdArgument = new FileIdArgument();
        AddArgument(fileIdArgument);
        
        this.SetHandler(async (fileIdArgumentValue) =>
        {
            await _dbContext.DeleteFile(fileIdArgumentValue);
            Console.WriteLine("File successfully deleted");
        }, fileIdArgument);
    }
}