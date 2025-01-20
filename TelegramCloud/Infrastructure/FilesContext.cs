using System.Data;
using Microsoft.Data.Sqlite;
using TelegramCloud.Models;
using File = TelegramCloud.Models.File;

namespace TelegramCloud.Infrastructure;

public interface IFilesContext
{
    Task<File?> GetFile(Guid fileId);
    IAsyncEnumerable<File> GetFiles();
    IAsyncEnumerable<FileChunk> GetFileChunks(Guid fileId);
    Task<Guid> InsertFile(string fileName, long fileSize, string encryptionKey, string encryptionIv);
    Task InsertFileChunk(Guid fileId, int chunkNumber, string telegramFileId, long size);
    Task DeleteFile(Guid fileId);
}

public class FilesContext : DatabaseContext, IFilesContext
{
    public async Task<File?> GetFile(Guid fileId)
    {
        await using var connection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ($"SELECT * FROM Files WHERE Id = '{fileId}'",
            connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        return sqlDataReader.Read()
            ? new File(
                fileId,
                sqlDataReader.GetString("Name"),
                sqlDataReader.GetInt64("Size"),
                sqlDataReader.GetString("EncryptionKey"),
                sqlDataReader.GetString("EncryptionIv"))
            : null;
    }

    public async IAsyncEnumerable<File> GetFiles()
    {
        await using var connection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ("SELECT * FROM Files",
            connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        while (sqlDataReader.Read())
        {
            yield return new File(
                sqlDataReader.GetGuid("Id"),
                sqlDataReader.GetString("Name"),
                sqlDataReader.GetInt64("Size"),
                sqlDataReader.GetString("EncryptionKey"),
                sqlDataReader.GetString("EncryptionIv")
            );
        }
    }

    public async IAsyncEnumerable<FileChunk> GetFileChunks(Guid fileId)
    {
        await using var connection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ($"""
          SELECT fc.TelegramFileId, fc.ChunkNumber FROM FileChunks fc
          JOIN Files f ON fc.FileId = f.Id
          WHERE f.Id = '{fileId}'
          """,
            connection);

        var sqlDataReader = sqlCommand.ExecuteReader();

        while (sqlDataReader.Read())
        {
            yield return new FileChunk(
                sqlDataReader.GetString("TelegramFileId"),
                sqlDataReader.GetInt32("ChunkNumber"));
        }
    }

    public async Task<Guid> InsertFile(string fileName, long fileSize, string encryptionKey, string encryptionIv)
    {
        var fileGuid = Guid.NewGuid();

        await using var connection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ($"""
          INSERT INTO Files (Id, Name, Size, EncryptionKey, EncryptionIv)
          VALUES ('{fileGuid}', '{fileName}', {fileSize}, '{encryptionKey}', '{encryptionIv}')
          """, connection);

        await sqlCommand.ExecuteNonQueryAsync();

        return fileGuid;
    }

    public async Task InsertFileChunk(Guid fileId, int chunkNumber, string telegramFileId, long size)
    {
        var fileChunkId = Guid.NewGuid();

        await using var connection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
        ($"""
          INSERT INTO FileChunks (Id, FileId, ChunkNumber, TelegramFileId, Size)
          VALUES ('{fileChunkId}', '{fileId}', {chunkNumber}, '{telegramFileId}', {size})
          """, connection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task DeleteFile(Guid fileId)
    {
        await using var connection = GetDatabaseConnection();

        var sqlCommand = new SqliteCommand
            ($"DELETE FROM Files WHERE Id = '{fileId}'", connection);

        await sqlCommand.ExecuteNonQueryAsync();

        sqlCommand = new SqliteCommand
            ($"DELETE FROM FileChunks WHERE FileId = '{fileId}'", connection);

        await sqlCommand.ExecuteNonQueryAsync();
    }

    protected override void InitializeDatabase(SqliteConnection connection)
    {
        CreateFilesTable(connection);
        CreateFileChunksTable(connection);
    }

    private static void CreateFilesTable(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("""
         CREATE TABLE IF NOT EXISTS Files(
             Id VARCHAR(36),
             Name NVARCHAR(256)
             Size INTEGER,
             EncryptionKey VARCHAR(50),
             EncryptionIv VARCHAR(30))
         """, connection);

        sqlCommand.ExecuteNonQuery();
    }

    private static void CreateFileChunksTable(SqliteConnection connection)
    {
        var sqlCommand = new SqliteCommand
        ("""
         CREATE TABLE IF NOT EXISTS FileChunks(
             Id VARCHAR(36),
             FileId VARCHAR(36),
             ChunkNumber INTEGER,
             TelegramFileId VARCHAR(100),
             Size INTEGER)
         """, connection);

        sqlCommand.ExecuteNonQuery();
    }
}