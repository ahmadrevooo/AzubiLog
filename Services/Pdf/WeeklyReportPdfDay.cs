namespace AzubiLog.Services.Pdf;

public sealed record WeeklyReportPdfDay(
    DateTime Date,
    string Label,
    IReadOnlyList<WeeklyReportPdfEntry> Entries);
