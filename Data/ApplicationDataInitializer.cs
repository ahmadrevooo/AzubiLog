using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace AzubiLog.Data;

public class ApplicationDataInitializer(ApplicationDbContext dbContext)
{
    private const string InitialMigrationId = "20260616200500_InitialCreate";
    private const string EfProductVersion = "10.0.0";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await BaselineLegacyDatabaseAsync(cancellationToken);

        // Apply pending EF Core migrations at startup.
        // User-specific defaults are created during registration.
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task BaselineLegacyDatabaseAsync(CancellationToken cancellationToken)
    {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            return;
        }

        var connection = dbContext.Database.GetDbConnection();
        var wasClosed = connection.State == System.Data.ConnectionState.Closed;

        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            var hasIdentityTable = await SqliteTableExistsAsync(connection, "AspNetUsers", cancellationToken);
            var hasMigrationHistory = await SqliteTableExistsAsync(connection, "__EFMigrationsHistory", cancellationToken);

            if (!hasIdentityTable || hasMigrationHistory)
            {
                return;
            }

            // Previous development databases may already have tables but no migration history.
            // If that legacy schema is present, mark the initial migration as applied so future schema
            // changes can be handled by normal EF Core migrations from this point onward.
            await ExecuteSqlAsync(
                connection,
                """
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                """,
                cancellationToken);
            await ExecuteSqlAsync(
                connection,
                $"""
                INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ('{InitialMigrationId}', '{EfProductVersion}');
                """,
                cancellationToken);
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<bool> SqliteTableExistsAsync(
        DbConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table' AND name = $tableName;
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }

    private static async Task ExecuteSqlAsync(
        DbConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
