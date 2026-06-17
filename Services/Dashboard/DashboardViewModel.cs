namespace AzubiLog.Services.Dashboard;

public sealed class DashboardViewModel
{
    public int CalendarWeek { get; init; }
    public string ApprenticeName { get; init; } = string.Empty;
    public decimal RecordedHours { get; init; }
    public int OpenTodoCount { get; init; }
    public IReadOnlyList<DashboardMetric> Metrics { get; init; } = [];
    public IReadOnlyList<DashboardQuickAction> QuickActions { get; init; } = [];
    public IReadOnlyList<DashboardWidgetItem> RecentReportEntries { get; init; } = [];
    public IReadOnlyList<DashboardWidgetItem> OpenTodos { get; init; } = [];
}
