using System.Globalization;

namespace AzubiLog.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private const decimal DefaultWeeklyTargetHours = 40m;

    public Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var recordedHours = 0m;
        var remainingHours = Math.Max(DefaultWeeklyTargetHours - recordedHours, 0m);
        var progressPercentage = DefaultWeeklyTargetHours == 0
            ? 0
            : (int)Math.Round(recordedHours / DefaultWeeklyTargetHours * 100m, MidpointRounding.AwayFromZero);
        var calendarWeek = ISOWeek.GetWeekOfYear(DateTime.Today);
        var openTodoCount = 0;

        var model = new DashboardViewModel
        {
            CalendarWeek = calendarWeek,
            WeeklyTargetHours = DefaultWeeklyTargetHours,
            RecordedHours = recordedHours,
            RemainingHours = remainingHours,
            ProgressPercentage = progressPercentage,
            OpenTodoCount = openTodoCount,
            Metrics =
            [
                new("DashboardCurrentWeek", calendarWeek.ToString(CultureInfo.CurrentCulture), "DashboardCurrentWeekDetail", "metric-accent-blue"),
                new("DashboardTargetHours", FormatHours(DefaultWeeklyTargetHours), "DashboardTargetHoursDetail", "metric-accent-green"),
                new("DashboardRecordedHours", FormatHours(recordedHours), "DashboardRecordedHoursDetail", "metric-accent-cyan"),
                new("DashboardRemainingHours", FormatHours(remainingHours), "DashboardRemainingHoursDetail", "metric-accent-orange"),
                new("DashboardProgress", $"{progressPercentage}%", "DashboardProgressDetail", "metric-accent-purple"),
                new("DashboardOpenTodos", openTodoCount.ToString(CultureInfo.CurrentCulture), "DashboardOpenTodosDetail", "metric-accent-red")
            ],
            QuickActions =
            [
                new("DashboardCreateEntry", "DashboardCreateEntryDescription", "report-entries/new", "+", "action-green", true),
                new("DashboardOpenWeeklyOverview", "DashboardOpenWeeklyOverviewDescription", "weekly-reports", "W", "action-blue", true),
                new("DashboardExportPdf", "DashboardExportPdfDescription", "weekly-reports/export", "PDF", "action-purple", true)
            ]
        };

        return Task.FromResult(model);
    }

    private static string FormatHours(decimal hours)
    {
        return string.Create(CultureInfo.CurrentCulture, $"{hours:0.#} h");
    }
}
