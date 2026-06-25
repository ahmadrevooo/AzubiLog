using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using AzubiLog.Data;

namespace AzubiLog.Components.Pages;

public partial class ClassMembersPage : ComponentBase
{
    private ApplicationUser? CurrentUser { get; set; }
    private List<ApplicationUser> Members { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await CurrentUserService.GetRequiredUserAsync();

        if (CurrentUser.Role != UserRole.Klassensprecher)
            return;

        Members = await DbContext.Users
            .Where(u => u.School == CurrentUser.School
                     && u.ClassName == CurrentUser.ClassName
                     && u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    private static string GetInitials(ApplicationUser user)
    {
        var first = string.IsNullOrWhiteSpace(user.FirstName) ? "" : user.FirstName[..1];
        var last = string.IsNullOrWhiteSpace(user.LastName) ? "" : user.LastName[..1];
        return $"{first}{last}".ToUpperInvariant();
    }
}
