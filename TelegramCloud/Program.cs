using System.CommandLine;
using TelegramCloud.Commands.Configurations;
using TelegramCloud.Commands.File;

var rootCommand = new RootCommand();

rootCommand.AddCommand(new ConfigurationCommand());
rootCommand.AddCommand(new FileCommand());

await rootCommand.InvokeAsync(args);