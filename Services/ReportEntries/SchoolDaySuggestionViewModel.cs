namespace AzubiLog.Services.ReportEntries;

public sealed class SchoolDaySuggestionViewModel
{
    public bool IsSchoolDay { get; init; }
    public string SubjectsText { get; init; } = string.Empty;
    public int? VocationalSchoolCategoryId { get; init; }
}
