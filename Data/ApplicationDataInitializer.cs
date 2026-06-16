using AzubiLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Data;

public class ApplicationDataInitializer(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    public const string SingleUserEmail = "apprentice@azubilog.local";

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
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

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
}
