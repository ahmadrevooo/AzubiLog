using AzubiLog.Configuration;
using AzubiLog.Data;
using AzubiLog.Interfaces;
using AzubiLog.Models;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services;

/// <summary>Manages user-owned report categories.</summary>
public sealed class CategoryService(IDbContextFactory<ApplicationDbContext> contextFactory) : ICategoryService
{
    /// <inheritdoc />
    public async Task EnsureDefaultsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existingNames = await context.Categories.AsNoTracking().Where(category => category.UserId == userId).Select(category => category.Name).ToListAsync(cancellationToken);
        var missing = DefaultCategories.All.Where(definition => !existingNames.Contains(definition.Name, StringComparer.OrdinalIgnoreCase)).Select((definition, index) => new Kategorie { UserId = userId, Name = definition.Name, ColorHex = definition.ColorHex, SortOrder = index });
        context.Categories.AddRange(missing);
        await context.SaveChangesAsync(cancellationToken);
    }
}
