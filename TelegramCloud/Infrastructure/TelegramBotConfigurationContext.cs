using System.Data;
using Microsoft.Data.Sqlite;
using TelegramCloud.Exceptions;
using TelegramCloud.Models;

namespace TelegramCloud.Infrastructure;

public interface ITelegramConfigurationContext
{
    (string? Token, int? ChatId) GetConfiguration();
    (string Token, int ChatId) GetRequiredConfiguration();
    Task SetBotToken(string token);
    Task SetChatId(int chatId);
}

public class TelegramBotConfigurationContext : DatabaseContext, ITelegramConfigurationContext
{
    public (string? Token, int? ChatId) GetConfiguration()
    {
        using var dbConnection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand("SELECT * FROM TelegramBotConfig", dbConnection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        return sqlDataReader.Read()
            ? (
                sqlDataReader.IsDBNull("Token") ? null : sqlDataReader.GetString("Token"),
                sqlDataReader.IsDBNull("ChatId") ? null : sqlDataReader.GetInt32("ChatId")
            )
            : default;
    }

    public (string Token, int ChatId) GetRequiredConfiguration()
    {
        var configuration = GetConfiguration();

        if (configuration.Token is null)
        {
            throw new ConfigurationException("Telegram bot token configuration is not set.");
        }

        if (configuration.ChatId is null)
        {
            throw new ConfigurationException("Telegram bot chat ID configuration is not set.");
        }

        return (configuration.Token, configuration.ChatId.Value);
    }

    public async Task SetBotToken(string token)
    {
        await using var dbConnection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ($"""
          UPDATE TelegramBotConfig
          SET Token = '{token}';

          INSERT INTO TelegramBotConfig (Token)
          SELECT '{token}'
          WHERE (SELECT Changes() = 0);
          """, dbConnection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task SetChatId(int chatId)
    {
        await using var dbConnection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ($"""
          UPDATE TelegramBotConfig
          SET ChatId = {chatId};

          INSERT INTO TelegramBotConfig (Token)
          SELECT {chatId}
          WHERE (SELECT Changes() = 0);
          """, dbConnection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    protected override void InitializeDatabase(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("CREATE TABLE IF NOT EXISTS TelegramBotConfig(Token VARCHAR(50) NULL, ChatId INTEGER NULL)",
            connection);

        sqlCommand.ExecuteNonQuery();
    }
}