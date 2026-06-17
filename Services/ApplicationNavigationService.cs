namespace AzubiLog.Services;

public class ApplicationNavigationService : IApplicationNavigationService
{
    private static readonly IReadOnlyList<NavigationItem> MainNavigation =
    [
        new("NavDashboard", "", "bi-speedometer-nav-menu", true),
        new("NavNewEntry", "report-entries/new", "bi-plus-square-fill-nav-menu", true),
        new("NavReportEntryMud", "report-entries/mud", "bi-plus-square-fill-nav-menu", true),
        new("NavReportEntryWriter", "report-entries/writer", "bi-plus-square-fill-nav-menu", true),
        new("NavWeeklyReports", "weekly-reports", "bi-journal-text-nav-menu", true),
        new("NavWeeklyReportsGrid", "weekly-reports/grid", "bi-grid-nav-menu", true),
        new("NavWeeklyReportsVertical", "weekly-reports/vertical", "bi-list-nested-nav-menu", true),
        new("NavWeeklyReportsMud", "weekly-reports/mud", "bi-grid-nav-menu", true),
        new("NavTodos", "todos", "bi-list-nested-nav-menu", true),
        new("NavHistory", "history", "bi-list-nested-nav-menu", false),
        new("NavSettings", "settings", "bi-gear-fill-nav-menu", true)
    ];

    public IReadOnlyList<NavigationItem> GetMainNavigation() => MainNavigation;
}
