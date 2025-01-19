using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using TelegramCloud;
using TelegramCloud.Commands.Configurations;
using TelegramCloud.Commands.File;
using TelegramCloud.Infrastructure;
using TelegramCloud.Services;

var rootCommand = new RootCommand();
rootCommand.AddCommand(new ConfigurationCommand());
rootCommand.AddCommand(new FileCommand());

var builder = new CommandLineBuilder(rootCommand)
    .UseDependencyInjection(services => 
    {
        services.AddTransient<IFileEncryptionService, FileEncryptionService>();
        services.AddTransient<ITelegramConfigurationContext, TelegramConfigurationContext>();
        services.AddTransient<IFilesContext, FilesContext>();
    })
    .UseExceptionHandler((exception, _) =>
    {
        Console.WriteLine(exception.Message);
    }, errorExitCode: 1)
    .UseDefaults();

await builder.Build().InvokeAsync(args);