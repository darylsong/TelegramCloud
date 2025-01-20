using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using TelegramCloud.Commands.Configurations;
using TelegramCloud.Commands.File;
using TelegramCloud.Extensions;
using TelegramCloud.Infrastructure;
using TelegramCloud.Services;

var rootCommand = new RootCommand();
rootCommand.AddCommand(new ConfigurationCommand());
rootCommand.AddCommand(new FileCommand());

var builder = new CommandLineBuilder(rootCommand)
    .UseDependencyInjection(services => 
    {
        services.AddTransient<ITelegramBot, TelegramBot>();
        services.AddTransient<IFileEncryptionService, FileEncryptionService>();
        services.AddTransient<ITelegramConfigurationContext, TelegramBotConfigurationContext>();
        services.AddTransient<IFilesContext, FilesContext>();
    })
    .UseExceptionHandler((exception, _) =>
    {
        Console.WriteLine(exception.Message);
    }, errorExitCode: 1)
    .UseDefaults();

await builder.Build().InvokeAsync(args);