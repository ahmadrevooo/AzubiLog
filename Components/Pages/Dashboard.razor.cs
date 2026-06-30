using System.Globalization;
using AzubiLog.Models;
using AzubiLog.Services.CalendarDayMarkers;
using AzubiLog.Services.Dashboard;
using AzubiLog.Services.Todos;
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
    private DateTime displayedMonth = DateTime.Today;
    private Dictionary<DateOnly, CalendarDayType> monthMarkers = [];
    private IReadOnlyList<TodoItemViewModel> OpenTodos { get; set; } = [];

    [Inject]
    private IDashboardService DashboardService { get; set; } = null!;

    [Inject]
    private ICalendarDayMarkerService CalendarDayMarkerService { get; set; } = null!;

    [Inject]
    private ITodoService TodoService { get; set; } = null!;

    protected DashboardViewModel? ViewModel { get; private set; }

    private CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

    private DateTime Today => DateTime.Today;

    private string TodayLabel => Today.ToString("dddd, dd.MM.yyyy", CurrentCulture);

    private string DisplayedMonthLabel => displayedMonth.ToString("MMMM yyyy", CurrentCulture);

    private string DisplayedMonthValue => displayedMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    private string SelectedTimerLabel => isCustomTimerSelected
        ? "Custom"
        : $"{selectedTimerMinutes} min";

    private string TimerDisplay => TimeSpan.FromSeconds(Math.Max(0, remainingSeconds)).ToString(@"hh\:mm\:ss", CurrentCulture);

    private bool CanStartTimer => !isTimerRunning && remainingSeconds > 0;

    private IReadOnlyList<DashboardCalendarDay> ProgressDays => BuildMonthDays(displayedMonth);

    private DateOnly? SelectedMarkerDate { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ViewModel = await DashboardService.GetDashboardAsync();
        OpenTodos = await TodoService.GetOpenTodosAsync(5);
        await LoadMarkersForDisplayedMonthAsync();
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

    private string GetProgressDayClass(DashboardCalendarDay day)
    {
        var classes = new List<string> { "progress-day", "day-marker", "day-marker-empty" };

        if (!day.IsCurrentMonth)
        {
            classes.Add("is-muted");
        }

        if (day.IsWeekend)
        {
            classes.Add("is-weekend");
        }

        if (SelectedMarkerDate == DateOnly.FromDateTime(day.Date))
        {
            classes.Add("is-selected");
        }

        if (GetMarkerType(day) is { } markerType)
        {
            classes.Remove("day-marker-empty");
            classes.Add(GetMarkerClass(markerType));
        }

        return string.Join(" ", classes);
    }

    private string GetProgressDayLabel(DashboardCalendarDay day)
    {
        var status = GetMarkerType(day) is { } markerType
            ? GetMarkerLabel(markerType)
            : "keine Markierung";

        return string.Create(CurrentCulture, $"{day.Date:D}: {status}");
    }

    private async Task ShowPreviousMonthAsync()
    {
        displayedMonth = displayedMonth.AddMonths(-1);
        SelectedMarkerDate = null;
        await LoadMarkersForDisplayedMonthAsync();
    }

    private async Task ShowNextMonthAsync()
    {
        displayedMonth = displayedMonth.AddMonths(1);
        SelectedMarkerDate = null;
        await LoadMarkersForDisplayedMonthAsync();
    }

    private async Task ShowCurrentMonthAsync()
    {
        displayedMonth = new DateTime(Today.Year, Today.Month, 1);
        SelectedMarkerDate = null;
        await LoadMarkersForDisplayedMonthAsync();
    }

    private async Task HandleDisplayedMonthChangedAsync(ChangeEventArgs args)
    {
        if (!DateTime.TryParseExact(
                args.Value?.ToString(),
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var selectedMonth))
        {
            return;
        }

        displayedMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        SelectedMarkerDate = null;
        await LoadMarkersForDisplayedMonthAsync();
    }

    private void ToggleMarkerMenu(DashboardCalendarDay day)
    {
        if (!day.IsCurrentMonth)
        {
            return;
        }

        var date = DateOnly.FromDateTime(day.Date);
        SelectedMarkerDate = SelectedMarkerDate == date ? null : date;
    }

    private async Task SetMarkerAsync(CalendarDayType type)
    {
        if (SelectedMarkerDate is not { } selectedDate)
        {
            return;
        }

        await CalendarDayMarkerService.SetMarkerAsync(selectedDate, type);
        SelectedMarkerDate = null;
        await LoadMarkersForDisplayedMonthAsync();
    }

    private async Task RemoveMarkerAsync()
    {
        if (SelectedMarkerDate is not { } selectedDate)
        {
            return;
        }

        await CalendarDayMarkerService.RemoveMarkerAsync(selectedDate);
        SelectedMarkerDate = null;
        await LoadMarkersForDisplayedMonthAsync();
    }

    private async Task LoadMarkersForDisplayedMonthAsync()
    {
        var markers = await CalendarDayMarkerService.GetMarkersForMonthAsync(
            displayedMonth.Year,
            displayedMonth.Month);

        monthMarkers = markers.ToDictionary(marker => marker.Date, marker => marker.Type);
    }

    private CalendarDayType? GetMarkerType(DashboardCalendarDay day)
    {
        return monthMarkers.TryGetValue(DateOnly.FromDateTime(day.Date), out var markerType)
            ? markerType
            : null;
    }

    private static string GetMarkerClass(CalendarDayType type)
    {
        return type switch
        {
            CalendarDayType.Workday => "day-marker-workday",
            CalendarDayType.SchoolDay => "day-marker-school-day",
            CalendarDayType.Vacation => "day-marker-vacation",
            CalendarDayType.SickLeave => "day-marker-sick-leave",
            CalendarDayType.Exam => "day-marker-exam",
            _ => "day-marker-empty"
        };
    }

    private static string GetMarkerLabel(CalendarDayType type)
    {
        return type switch
        {
            CalendarDayType.Workday => "Arbeitstag",
            CalendarDayType.SchoolDay => "Schultag",
            CalendarDayType.Vacation => "Urlaub",
            CalendarDayType.SickLeave => "Krankheit",
            CalendarDayType.Exam => "Prüfung / Klausur",
            _ => "keine Markierung"
        };
    }

    private string GetTodoDueDateLabel(DateTime? dueDate)
    {
        if (dueDate is null)
        {
            return "Kein Datum";
        }

        return dueDate.Value.Date == Today
            ? "Heute"
            : dueDate.Value.ToString("dd.MM.yyyy", CurrentCulture);
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

    private sealed record DashboardCalendarDay(DateTime Date, bool IsCurrentMonth, bool IsWeekend);

    private sealed record TimerDurationOption(string Label, int Minutes, bool IsCustom);
}
