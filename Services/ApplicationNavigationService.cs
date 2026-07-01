namespace AzubiLog.Services;

public class ApplicationNavigationService : IApplicationNavigationService
{
    private static readonly IReadOnlyList<NavigationItem> MainNavigation =
    [
        new("NavDashboard", "", "bi-speedometer-nav-menu", true),
        new("NavNewEntry", "report-entries/new", "bi-plus-square-fill-nav-menu", true),
        new("NavWeeklyReports", "weekly-reports/grid", "bi-journal-text-nav-menu", true),
        new("NavTodos", "todos", "bi-list-nested-nav-menu", true),
        new("TimetableNavItem", "timetable", "bi-calendar-week-nav-menu", true),
        new("NavSettings", "settings", "bi-gear-fill-nav-menu", true)
    ];

    public IReadOnlyList<NavigationItem> GetMainNavigation() => MainNavigation;
}
