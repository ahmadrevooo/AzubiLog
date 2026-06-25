namespace AzubiLog.Models;

public class ClassTimetableEntry
{
    public int Id { get; set; }
    public string School { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public string SubjectsText { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<TimetableCancellation> Cancellations { get; set; } = new();
}
