namespace AzubiLog.Services.ReportEntries;

public sealed record ReportEntryListItem(
    int Id,
    string Title,
    string CategoryName,
    decimal Hours,
    string Status);
