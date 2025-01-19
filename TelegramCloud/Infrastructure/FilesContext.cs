using System.Data;
using Microsoft.Data.Sqlite;
using TelegramCloud.Exceptions;
using TelegramCloud.Models;

namespace TelegramCloud.Infrastructure;

public interface IFilesContext
{
    Task<Guid> InsertFile(string fileName, long fileSize, string encryptionKey, string encryptionIv);
    Task DeleteFile(Guid fileId);
    Task InsertFileChunk(Guid fileId, int chunkNumber, string telegramFileId, long size);
    IEnumerable<FileDto> GetFiles();
    IEnumerable<FileChunkDto> GetFileChunks(Guid fileId);
    FileDto? GetFile(Guid fileId);
    TelegramBotConfigDto? GetTelegramBotConfig();
    TelegramBotConfigDto GetRequiredTelegramBotConfig();
    Task SetTelegramBotTokenConfig(string token);
    Task SetTelegramBotChatIdConfig(int chatId);
}

public class FilesContext : IFilesContext
{
    private const string ConnectionString = "Data Source=telegramcloud.sqlite3";

    public FilesContext()
    {
        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        InitializeDatabase(connection);
    }

    public async Task<Guid> InsertFile(string fileName, long fileSize, string encryptionKey, string encryptionIv)
    {
        var fileGuid = Guid.NewGuid();

        await using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        var sqlCommand = new SqliteCommand
        ($"INSERT INTO Files (Id, Name, Size, EncryptionKey, EncryptionIv) VALUES ('{fileGuid}', '{fileName}', {fileSize}, '{encryptionKey}', '{encryptionIv}')",
            connection);

        await sqlCommand.ExecuteNonQueryAsync();

        return fileGuid;
    }

    public async Task DeleteFile(Guid fileId)
    {
        await using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        var sqlCommand = new SqliteCommand
            ($"DELETE FROM Files WHERE Id = '{fileId}'", connection);

        await sqlCommand.ExecuteNonQueryAsync();

        sqlCommand = new SqliteCommand
            ($"DELETE FROM FileChunks WHERE FileId = '{fileId}'", connection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task InsertFileChunk(Guid fileId, int chunkNumber, string telegramFileId, long size)
    {
        var fileChunkId = Guid.NewGuid();

        await using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        var sqlCommand = new SqliteCommand
        ($"INSERT INTO FileChunks (Id, FileId, ChunkNumber, TelegramFileId, Size) VALUES ('{fileChunkId}', '{fileId}', {chunkNumber}, '{telegramFileId}', {size})",
            connection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public IEnumerable<FileDto> GetFiles()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sqlCommand = new SqliteCommand
        ("SELECT * FROM Files",
            connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        while (sqlDataReader.Read())
        {
            yield return new FileDto(
                sqlDataReader.GetGuid("Id"),
                sqlDataReader.GetString("Name"),
                sqlDataReader.GetInt64("Size"),
                sqlDataReader.GetString("EncryptionKey"),
                sqlDataReader.GetString("EncryptionIv")
            );
        }
    }

    public IEnumerable<FileChunkDto> GetFileChunks(Guid fileId)
    {
        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        var sqlCommand = new SqliteCommand
        ($"SELECT fc.TelegramFileId, fc.ChunkNumber FROM FileChunks fc JOIN Files f ON fc.FileId = f.Id WHERE f.Id = '{fileId}'",
            connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        while (sqlDataReader.Read())
        {
            yield return new FileChunkDto(
                sqlDataReader.GetString("TelegramFileId"),
                sqlDataReader.GetInt32("ChunkNumber"));
        }
    }

    public FileDto? GetFile(Guid fileId)
    {
        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        var sqlCommand = new SqliteCommand
        ($"SELECT * FROM Files WHERE Id = '{fileId}'",
            connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        return sqlDataReader.Read()
            ? new FileDto(
                fileId,
                sqlDataReader.GetString("Name"),
                sqlDataReader.GetInt64("Size"),
                sqlDataReader.GetString("EncryptionKey"),
                sqlDataReader.GetString("EncryptionIv"))
            : null;
    }

    public TelegramBotConfigDto? GetTelegramBotConfig()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sqlCommand = new SqliteCommand("SELECT * FROM TelegramBotConfig", connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        return sqlDataReader.Read()
            ? new TelegramBotConfigDto(
                sqlDataReader.IsDBNull("Token") ? null : sqlDataReader.GetString("Token"),
                sqlDataReader.IsDBNull("ChatId") ? null : sqlDataReader.GetInt32("ChatId"))
            : null;
    }

    public TelegramBotConfigDto GetRequiredTelegramBotConfig()
    {
        var telegramBotConfig = GetTelegramBotConfig();

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
        await using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sqlCommand = new SqliteCommand
        ($"""
          UPDATE TelegramBotConfig
          SET Token = '{token}';

          INSERT INTO TelegramBotConfig (Token)
          SELECT '{token}'
          WHERE (SELECT Changes() = 0);
          """, connection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task SetTelegramBotChatIdConfig(int chatId)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sqlCommand = new SqliteCommand
        ($"""
          UPDATE TelegramBotConfig
          SET ChatId = {chatId};

          INSERT INTO TelegramBotConfig (Token)
          SELECT {chatId}
          WHERE (SELECT Changes() = 0);
          """, connection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    private static void InitializeDatabase(SqliteConnection connection)
    {
        CreateTelegramBotConfigTable(connection);
        CreateFilesTable(connection);
        CreateFileChunksTable(connection);
    }

    private static void CreateTelegramBotConfigTable(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("CREATE TABLE IF NOT EXISTS TelegramBotConfig(Token VARCHAR(50) NULL, ChatId INTEGER NULL)",
            connection);

        sqlCommand.ExecuteNonQuery();
    }

    private static void CreateFilesTable(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("CREATE TABLE IF NOT EXISTS Files(Id VARCHAR(36), Name NVARCHAR(256), Size INTEGER, EncryptionKey VARCHAR(50), EncryptionIv VARCHAR(30))",
            connection);

        sqlCommand.ExecuteNonQuery();
    }

    private static void CreateFileChunksTable(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("CREATE TABLE IF NOT EXISTS FileChunks(Id VARCHAR(36), FileId VARCHAR(36), ChunkNumber INTEGER, TelegramFileId VARCHAR(100), Size INTEGER)",
            connection);

        sqlCommand.ExecuteNonQuery();
    }
}