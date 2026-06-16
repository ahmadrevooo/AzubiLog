using AzubiLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace AzubiLog.Data;

public class ApplicationDataInitializer(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    public const string SingleUserEmail = "apprentice@azubilog.local";
    private const string InitialMigrationId = "20260616200500_InitialCreate";
    private const string EfProductVersion = "10.0.0";

    private static readonly IReadOnlyList<(string Name, string ColorHex)> DefaultCategories =
    [
        ("Internal Activities", "#10b981"),
        ("Vocational School", "#0ea5e9"),
        ("Vacation", "#f59e0b"),
        ("Sick Leave", "#ef4444"),
        ("Overtime", "#8b5cf6")
    ];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await BaselineLegacyDatabaseAsync(cancellationToken);

        // Apply pending EF Core migrations before seeding application defaults.
        // Keep schema changes in migrations; this initializer only prepares runtime seed data.
        await dbContext.Database.MigrateAsync(cancellationToken);

        var user = await userManager.FindByEmailAsync(SingleUserEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = SingleUserEmail,
                Email = SingleUserEmail,
                EmailConfirmed = true,
                FirstName = "Apprentice",
                LastName = "User",
                IsActive = true
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create default apprentice user: {errors}");
            }
        }

        var existingCategoryNames = await dbContext.Categories
            .Where(category => category.UserId == user.Id)
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);

        var sortOrder = await dbContext.Categories
            .Where(category => category.UserId == user.Id)
            .CountAsync(cancellationToken);

        foreach (var (name, colorHex) in DefaultCategories)
        {
            if (existingCategoryNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            dbContext.Categories.Add(new Category
            {
                UserId = user.Id,
                Name = name,
                ColorHex = colorHex,
                SortOrder = ++sortOrder
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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
