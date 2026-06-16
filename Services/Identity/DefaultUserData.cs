using AzubiLog.Data;
using AzubiLog.Models;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.Identity;

public sealed class DefaultUserData(ApplicationDbContext dbContext)
{
    public static readonly IReadOnlyList<(string Name, string ColorHex)> Categories =
    [
        ("Internal Activities", "#10b981"),
        ("Vocational School", "#0ea5e9"),
        ("Vacation", "#f59e0b"),
        ("Sick Leave", "#ef4444"),
        ("Overtime", "#8b5cf6")
    ];

    public async Task EnsureDefaultCategoriesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var existingCategoryNames = await dbContext.Categories
            .Where(category => category.UserId == userId)
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);

        var sortOrder = await dbContext.Categories
            .Where(category => category.UserId == userId)
            .CountAsync(cancellationToken);

        foreach (var (name, colorHex) in Categories)
        {
            if (existingCategoryNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            dbContext.Categories.Add(new Category
            {
                UserId = userId,
                Name = name,
                ColorHex = colorHex,
                SortOrder = ++sortOrder
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
