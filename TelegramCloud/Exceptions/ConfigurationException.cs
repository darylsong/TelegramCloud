namespace TelegramCloud.Exceptions;

public class ConfigurationException(string errorMessage) : Exception
{
    public string ErrorMessage = errorMessage;
}