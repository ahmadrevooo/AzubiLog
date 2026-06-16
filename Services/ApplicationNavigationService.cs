namespace AzubiLog.Services;

public class ApplicationNavigationService : IApplicationNavigationService
{
    private static readonly IReadOnlyList<NavigationItem> MainNavigation =
    [
        new("NavDashboard", "", "bi-speedometer-nav-menu", true),
        new("NavNewEntry", "report-entries/new", "bi-plus-square-fill-nav-menu", false),
        new("NavWeeklyReports", "weekly-reports", "bi-journal-text-nav-menu", false),
        new("NavHistory", "history", "bi-list-nested-nav-menu", false),
        new("NavSettings", "settings", "bi-gear-fill-nav-menu", false)
    ];

    public IReadOnlyList<NavigationItem> GetMainNavigation() => MainNavigation;
}
