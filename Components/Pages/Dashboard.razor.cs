using System.Globalization;
using AzubiLog.Services.Dashboard;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class Dashboard : ComponentBase
{
    private static readonly string[] WeekdayLabels = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];
    private static readonly string[] TimerDurations = ["15 min", "25 min", "45 min", "60 min", "Custom"];
    private static readonly TimeSpan SelectedDuration = TimeSpan.FromMinutes(25);

    [Inject]
    private IDashboardService DashboardService { get; set; } = null!;

    protected DashboardViewModel? ViewModel { get; private set; }

    private CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

    private DateTime Today => DateTime.Today;

    private string TodayLabel => Today.ToString("dddd, dd.MM.yyyy", CurrentCulture);

    private string CurrentMonthLabel => Today.ToString("MMMM yyyy", CurrentCulture);

    private string SelectedTimerDuration => "25 min";

    private string TimerDisplay => SelectedDuration.ToString(@"hh\:mm\:ss", CurrentCulture);

    private IReadOnlyList<DashboardCalendarDay> CalendarDays => BuildMonthDays(Today);

    private IReadOnlyList<DashboardCalendarDay> ProgressDays => BuildMonthDays(Today);

    private ISet<DateOnly> EntryDates => new HashSet<DateOnly>
    {
        DateOnly.FromDateTime(GetStartOfWeek(Today).AddDays(3)),
        DateOnly.FromDateTime(GetStartOfWeek(Today).AddDays(4))
    };

    protected override async Task OnInitializedAsync()
    {
        ViewModel = await DashboardService.GetDashboardAsync();
    }

    protected string GetWelcomeHeading()
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.ApprenticeName))
        {
            return Localizer["DashboardWelcome"];
        }

        return string.Format(
            CurrentCulture,
            Localizer["DashboardWelcomeNamed"],
            ViewModel.ApprenticeName);
    }

    private string GetCalendarDayClass(DashboardCalendarDay day)
    {
        var classes = new List<string> { "month-day" };

        if (!day.IsCurrentMonth)
        {
            classes.Add("is-muted");
        }

        if (day.IsWeekend)
        {
            classes.Add("is-weekend");
        }

        if (DateOnly.FromDateTime(day.Date) == DateOnly.FromDateTime(Today))
        {
            classes.Add("is-today");
        }

        return string.Join(" ", classes);
    }

    private string GetProgressDayClass(DashboardCalendarDay day)
    {
        var classes = new List<string> { "progress-day" };

        if (!day.IsCurrentMonth)
        {
            classes.Add("is-muted");
        }

        if (EntryDates.Contains(DateOnly.FromDateTime(day.Date)))
        {
            classes.Add("has-entry");
        }

        return string.Join(" ", classes);
    }

    private string GetProgressDayLabel(DashboardCalendarDay day)
    {
        var status = EntryDates.Contains(DateOnly.FromDateTime(day.Date))
            ? "Eintrag vorhanden"
            : "kein Eintrag";

        return string.Create(CurrentCulture, $"{day.Date:D}: {status}");
    }

    private static IReadOnlyList<DashboardCalendarDay> BuildMonthDays(DateTime referenceDate)
    {
        var firstOfMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var startOffset = ((int)firstOfMonth.DayOfWeek + 6) % 7;
        var gridStart = firstOfMonth.AddDays(-startOffset);
        var daysInGrid = startOffset + DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);
        var fullWeeks = (int)Math.Ceiling(daysInGrid / 7d) * 7;

        return Enumerable.Range(0, fullWeeks)
            .Select(offset =>
            {
                var date = gridStart.AddDays(offset);

                return new DashboardCalendarDay(
                    date,
                    date.Month == referenceDate.Month,
                    date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
            })
            .ToList();
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }

    private sealed record DashboardCalendarDay(DateTime Date, bool IsCurrentMonth, bool IsWeekend);
}
