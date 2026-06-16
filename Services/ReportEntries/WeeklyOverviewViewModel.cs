namespace AzubiLog.Services.ReportEntries;

public sealed class WeeklyOverviewViewModel
{
    public int CalendarWeek { get; init; }
    public decimal WeeklyTargetHours { get; init; }
    public decimal RecordedHours { get; init; }
    public decimal RemainingHours { get; init; }
    public IReadOnlyList<WeeklyOverviewDay> Days { get; init; } = [];
}
