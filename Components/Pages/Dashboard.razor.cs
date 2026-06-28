using System.Globalization;
using AzubiLog.Services.Dashboard;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    private static readonly string[] WeekdayLabels = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];
    private static readonly TimerDurationOption[] TimerDurations =
    [
        new("15 min", 15, false),
        new("25 min", 25, false),
        new("45 min", 45, false),
        new("60 min", 60, false),
        new("Custom", 25, true)
    ];

    private int selectedTimerMinutes = 25;
    private int customTimerMinutes = 25;
    private int remainingSeconds = 25 * 60;
    private bool isCustomTimerSelected;
    private bool isTimerRunning;
    private PeriodicTimer? dashboardTimer;
    private CancellationTokenSource? timerCancellationTokenSource;

    [Inject]
    private IDashboardService DashboardService { get; set; } = null!;

    protected DashboardViewModel? ViewModel { get; private set; }

    private CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

    private DateTime Today => DateTime.Today;

    private string TodayLabel => Today.ToString("dddd, dd.MM.yyyy", CurrentCulture);

    private string CurrentMonthLabel => Today.ToString("MMMM yyyy", CurrentCulture);

    private string SelectedTimerLabel => isCustomTimerSelected
        ? "Custom"
        : $"{selectedTimerMinutes} min";

    private string TimerDisplay => TimeSpan.FromSeconds(Math.Max(0, remainingSeconds)).ToString(@"hh\:mm\:ss", CurrentCulture);

    private bool CanStartTimer => !isTimerRunning && remainingSeconds > 0;

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

    public async ValueTask DisposeAsync()
    {
        await StopTimerAsync(updateUi: false);
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

    private void SelectTimerDuration(TimerDurationOption duration)
    {
        if (isTimerRunning)
        {
            return;
        }

        isCustomTimerSelected = duration.IsCustom;
        selectedTimerMinutes = duration.IsCustom ? GetValidCustomTimerMinutes() : duration.Minutes;
        ResetRemainingTimerSeconds();
    }

    private void HandleCustomTimerMinutesChanged(ChangeEventArgs args)
    {
        if (!int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CurrentCulture, out var minutes)
            && !int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes))
        {
            return;
        }

        if (minutes <= 0)
        {
            return;
        }

        customTimerMinutes = minutes;

        if (isTimerRunning)
        {
            return;
        }

        isCustomTimerSelected = true;
        selectedTimerMinutes = customTimerMinutes;
        ResetRemainingTimerSeconds();
    }

    private async Task StartTimerAsync()
    {
        if (isTimerRunning || remainingSeconds <= 0)
        {
            return;
        }

        await StopTimerAsync();

        isTimerRunning = true;
        timerCancellationTokenSource = new CancellationTokenSource();
        dashboardTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunTimerAsync(timerCancellationTokenSource.Token);
    }

    private async Task PauseTimer()
    {
        await StopTimerAsync();
    }

    private async Task ResetTimer()
    {
        await StopTimerAsync();
        ResetRemainingTimerSeconds();
    }

    private async Task RunTimerAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (dashboardTimer is not null
                && await dashboardTimer.WaitForNextTickAsync(cancellationToken)
                && remainingSeconds > 0)
            {
                remainingSeconds--;

                if (remainingSeconds <= 0)
                {
                    await StopTimerAsync();
                }

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private Task StopTimerAsync(bool updateUi = true)
    {
        if (!isTimerRunning && dashboardTimer is null && timerCancellationTokenSource is null)
        {
            return Task.CompletedTask;
        }

        isTimerRunning = false;
        timerCancellationTokenSource?.Cancel();
        timerCancellationTokenSource?.Dispose();
        timerCancellationTokenSource = null;
        dashboardTimer?.Dispose();
        dashboardTimer = null;

        return updateUi ? InvokeAsync(StateHasChanged) : Task.CompletedTask;
    }

    private void ResetRemainingTimerSeconds()
    {
        selectedTimerMinutes = isCustomTimerSelected ? GetValidCustomTimerMinutes() : selectedTimerMinutes;
        remainingSeconds = Math.Max(1, selectedTimerMinutes) * 60;
    }

    private int GetValidCustomTimerMinutes()
    {
        return customTimerMinutes > 0 ? customTimerMinutes : 25;
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

    private sealed record TimerDurationOption(string Label, int Minutes, bool IsCustom);
}
