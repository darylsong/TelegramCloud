using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using TelegramCloud.Infrastructure;

namespace TelegramCloud.Commands.File.List;

public class ListFilesCommand : Command
{
    public ListFilesCommand() : base("list", "List all uploaded files")
    {
        Handler = CommandHandler.Create<IFilesContext>(async filesContext =>
        {
            var files = await filesContext.GetFiles().ToListAsync();

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