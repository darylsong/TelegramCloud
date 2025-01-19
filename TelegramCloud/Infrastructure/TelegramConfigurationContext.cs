using System.Data;
using Microsoft.Data.Sqlite;
using TelegramCloud.Exceptions;
using TelegramCloud.Models;

namespace TelegramCloud.Infrastructure;

public interface ITelegramConfigurationContext
{
    TelegramBotConfigDto? GetConfiguration();
    TelegramBotConfigDto GetRequiredTelegramBotConfig();
    Task SetTelegramBotTokenConfig(string token);
    Task SetTelegramBotChatIdConfig(int chatId);
}

public class TelegramConfigurationContext : DatabaseContext, ITelegramConfigurationContext
{
    public TelegramBotConfigDto? GetConfiguration()
    {
        using var dbConnection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand("SELECT * FROM TelegramBotConfig", dbConnection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        return sqlDataReader.Read()
            ? new TelegramBotConfigDto(
                sqlDataReader.IsDBNull("Token") ? null : sqlDataReader.GetString("Token"),
                sqlDataReader.IsDBNull("ChatId") ? null : sqlDataReader.GetInt32("ChatId"))
            : null;
    }

    public TelegramBotConfigDto GetRequiredTelegramBotConfig()
    {
        var telegramBotConfig = GetConfiguration();

        if (telegramBotConfig is null)
        {
            throw new ConfigurationException("Telegram bot configuration is not set.");
        }

        if (telegramBotConfig.Token is null)
        {
            throw new ConfigurationException("Telegram bot token configuration is not set.");
        }

        if (telegramBotConfig.ChatId is null)
        {
            throw new ConfigurationException("Telegram bot chat ID configuration is not set.");
        }

        return telegramBotConfig;
    }

    public async Task SetTelegramBotTokenConfig(string token)
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

    public async Task SetTelegramBotChatIdConfig(int chatId)
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
        CreateTelegramBotConfigTable(connection);
    }

    private static void CreateTelegramBotConfigTable(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("CREATE TABLE IF NOT EXISTS TelegramBotConfig(Token VARCHAR(50) NULL, ChatId INTEGER NULL)",
            connection);

        sqlCommand.ExecuteNonQuery();
    }
}