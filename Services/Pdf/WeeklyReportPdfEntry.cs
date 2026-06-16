namespace AzubiLog.Services.Pdf;

public sealed record WeeklyReportPdfEntry(
    DateTime Date,
    string CategoryName,
    string Title,
    string Description,
    string Notes,
    string? Subject,
    string DayType,
    DateTime StartTime,
    DateTime EndTime,
    decimal Hours);
