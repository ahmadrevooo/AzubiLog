namespace AzubiLog.Services.ReportEntries;

public sealed class SchoolDaySuggestionViewModel
{
    public bool IsSchoolDay { get; init; }
    public string SubjectsText { get; init; } = string.Empty;
    public int? VocationalSchoolCategoryId { get; init; }
    public List<SchoolSubjectEntry> Subjects { get; init; } = [];
}

public sealed class SchoolSubjectEntry
{
    public string Fach { get; init; } = string.Empty;
    public string Lehrer { get; init; } = string.Empty;
    public string? Raum { get; init; }
    public string? StartTime { get; init; }
    public string? EndTime { get; init; }
    public int? BreakMinutes { get; init; }
    public bool Entfall { get; init; }
}
