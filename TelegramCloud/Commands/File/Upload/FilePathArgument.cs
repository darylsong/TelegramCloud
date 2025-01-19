using System.CommandLine;

namespace TelegramCloud.Commands.File.Upload;

public class FilePathArgument() : Argument<string>(name: "filePath",
    description: "Path to file to be uploaded");