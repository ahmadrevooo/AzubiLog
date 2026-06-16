namespace AzubiLog.Services.ReportEntries;

public sealed class DailySummaryViewModel
{
    public DateTime Date { get; init; } = DateTime.Today;
    public int EntryCount { get; init; }
    public decimal TotalHours { get; init; }
    public IReadOnlyList<ReportEntryListItem> Entries { get; init; } = [];
}
