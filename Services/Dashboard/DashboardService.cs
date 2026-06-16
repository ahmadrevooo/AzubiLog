using System.Globalization;
using AzubiLog.Services.Todos;

namespace AzubiLog.Services.Dashboard;

public class DashboardService(ITodoService todoService) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var recordedHours = 0m;
        var calendarWeek = ISOWeek.GetWeekOfYear(DateTime.Today);
        var openTodoCount = await todoService.GetOpenTodoCountAsync(cancellationToken);
        var openTodos = await todoService.GetOpenTodosAsync(5, cancellationToken);

        var model = new DashboardViewModel
        {
            CalendarWeek = calendarWeek,
            RecordedHours = recordedHours,
            OpenTodoCount = openTodoCount,
            Metrics =
            [
                new("DashboardCurrentWeek", calendarWeek.ToString(CultureInfo.CurrentCulture), "DashboardCurrentWeekDetail", "metric-accent-blue"),
                new("DashboardRecordedHours", FormatHours(recordedHours), "DashboardRecordedHoursDetail", "metric-accent-cyan"),
                new("DashboardOpenTodos", openTodoCount.ToString(CultureInfo.CurrentCulture), "DashboardOpenTodosDetail", "metric-accent-red")
            ],
            QuickActions =
            [
                new("DashboardCreateEntry", "DashboardCreateEntryDescription", "report-entries/new", "+", "action-green", true),
                new("DashboardOpenWeeklyOverview", "DashboardOpenWeeklyOverviewDescription", "weekly-reports", "W", "action-blue", true)
            ],
            OpenTodos = openTodos
                .Select(todo => new DashboardWidgetItem(
                    todo.Title,
                    string.IsNullOrWhiteSpace(todo.Description) ? "-" : todo.Description,
                    todo.DueDate.HasValue ? todo.DueDate.Value.ToString("d", CultureInfo.CurrentCulture) : "-"))
                .ToList()
        };

        return model;
    }

    private static string FormatHours(decimal hours)
    {
        return string.Create(CultureInfo.CurrentCulture, $"{hours:0.#} h");
    }
}
