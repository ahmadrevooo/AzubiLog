namespace AzubiLog.Models;

/// <summary>Represents one daily apprenticeship activity or absence.</summary>
public class BerichtEintrag
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int? TrainerId { get; set; }
    public Ausbilder? Trainer { get; set; }
    public int CategoryId { get; set; }
    public Kategorie Category { get; set; } = null!;
    public int WeeklyReportId { get; set; }
    public Wochenbericht WeeklyReport { get; set; } = null!;
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public decimal WorkingHours { get; set; }
    public bool IsSchoolDay { get; set; }
    public string? Subject { get; set; }
    public bool IsDraft { get; set; } = true;
    public DateTimeOffset? LastAutoSaveAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
