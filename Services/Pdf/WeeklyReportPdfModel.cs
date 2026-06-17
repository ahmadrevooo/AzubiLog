namespace AzubiLog.Services.Pdf;

public sealed class WeeklyReportPdfModel
{
    public string ApprenticeName { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string Occupation { get; init; } = string.Empty;
    public int TrainingYear { get; init; }
    public string TrainerName { get; init; } = string.Empty;
    public int CalendarWeek { get; init; }
    public int Year { get; init; }
    public decimal TotalHours { get; init; }
    public IReadOnlyList<WeeklyReportPdfDay> Days { get; init; } = [];
}
