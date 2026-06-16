namespace AzubiLog.Services.ReportEntries;

public sealed class WeeklyOverviewDay
{
    public DateTime Date { get; init; }
    public string DayKey { get; init; } = string.Empty;
    public int EntryCount { get; init; }
    public decimal TotalHours { get; init; }
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyList<ReportEntryListItem> Entries { get; init; } = [];
}
