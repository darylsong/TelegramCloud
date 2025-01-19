using System.CommandLine;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.File.List;

public class ListFilesCommand : Command
{
    private readonly FilesContext _dbContext = new();

    public ListFilesCommand() : base("list", "List all uploaded files")
    {
        this.SetHandler(_ =>
        {
            var files = _dbContext.GetFiles().ToList();

            if (files.Count == 0)
            {
                Console.WriteLine("No files found.");
                return;
            }

            Console.WriteLine("|{0,37}|{1,30}|{2,15}|", "Id", "Name", "Size (bytes)");

            foreach (var file in files)
            {
                Console.WriteLine("|{0,37}|{1,30}|{2,15}|", file.Id, file.Name, file.Size);
            }
        });
    }
}