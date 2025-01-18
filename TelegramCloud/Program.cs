using System.CommandLine;
using System.CommandLine.Builder;
using TelegramCloud.Commands.Configurations;
using TelegramCloud.Commands.File;

var rootCommand = new RootCommand();

rootCommand.AddCommand(new ConfigurationCommand());
rootCommand.AddCommand(new FileCommand());

new CommandLineBuilder(rootCommand)
    .UseExceptionHandler((exception, _) =>
    {
        Console.WriteLine(exception.Message);
    }, errorExitCode: 1)
    .Build();

await rootCommand.InvokeAsync(args);