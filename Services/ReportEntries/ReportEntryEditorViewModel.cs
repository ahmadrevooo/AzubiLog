namespace AzubiLog.Services.ReportEntries;

public sealed class ReportEntryEditorViewModel
{
    public ReportEntryFormModel Entry { get; init; } = new();
    public IReadOnlyList<ReportEntryOption> Categories { get; init; } = [];
    public IReadOnlyList<ReportEntryOption> Trainers { get; init; } = [];
    public DailySummaryViewModel DailySummary { get; init; } = new();
    public WeeklyOverviewViewModel WeeklyOverview { get; init; } = new();
    public SchoolDaySuggestionViewModel? SchoolDaySuggestion { get; init; }
    public decimal CalculatedHours { get; init; }
    public bool RestoredDraft { get; init; }
}
