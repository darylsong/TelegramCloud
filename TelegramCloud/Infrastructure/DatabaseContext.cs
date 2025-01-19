using Microsoft.Data.Sqlite;

namespace TelegramCloud.Infrastructure;

public abstract class DatabaseContext
{
    private const string ConnectionString = "Data Source=telegramcloud.sqlite3";

    protected DatabaseContext()
    {
        using var connection = GetDatabaseConnection();

        EnsureDatabaseCreated(connection);
    }

    protected SqliteConnection GetDatabaseConnection()
    {
        var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        return connection;
    }

    private void EnsureDatabaseCreated(SqliteConnection connection)
    {
        InitializeDatabase(connection);
    }

    protected abstract void InitializeDatabase(SqliteConnection connection);
}