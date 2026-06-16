namespace AzubiLog.Models;

public class ReportEntry
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int? TrainerId { get; set; }
    public Trainer? Trainer { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public int WeeklyReportId { get; set; }
    public WeeklyReport WeeklyReport { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string DayType { get; set; } = "Company";
    public string? OrderNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal? Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
