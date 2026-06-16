namespace AzubiLog.Services;

public class ApplicationNavigationService : IApplicationNavigationService
{
    private static readonly IReadOnlyList<NavigationItem> MainNavigation =
    [
        new("NavHome", "", "bi-house-door-fill-nav-menu", true),
        new("NavDashboard", "dashboard", "bi-speedometer-nav-menu", false),
        new("NavWeeklyReports", "weekly-reports", "bi-journal-text-nav-menu", false),
        new("NavNewEntry", "report-entries/new", "bi-plus-square-fill-nav-menu", false),
        new("NavCategories", "categories", "bi-tags-fill-nav-menu", false),
        new("NavTrainers", "trainers", "bi-person-badge-fill-nav-menu", false),
        new("NavSettings", "settings", "bi-gear-fill-nav-menu", false)
    ];

    public IReadOnlyList<NavigationItem> GetMainNavigation() => MainNavigation;
}
