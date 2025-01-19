using System.CommandLine;

namespace TelegramCloud.Commands.File.Shared;

public class FileIdArgument() : Argument<Guid>(name: "fileId",
    description: "File ID");