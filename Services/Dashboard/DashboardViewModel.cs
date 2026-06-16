namespace AzubiLog.Services.Dashboard;

public sealed class DashboardViewModel
{
    public int CalendarWeek { get; init; }
    public decimal WeeklyTargetHours { get; init; }
    public decimal RecordedHours { get; init; }
    public decimal RemainingHours { get; init; }
    public int ProgressPercentage { get; init; }
    public int OpenTodoCount { get; init; }
    public IReadOnlyList<DashboardMetric> Metrics { get; init; } = [];
    public IReadOnlyList<DashboardQuickAction> QuickActions { get; init; } = [];
    public IReadOnlyList<DashboardWidgetItem> RecentReportEntries { get; init; } = [];
    public IReadOnlyList<DashboardWidgetItem> OpenTodos { get; init; } = [];
}
