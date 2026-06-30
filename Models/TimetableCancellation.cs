namespace AzubiLog.Models;

public class TimetableCancellation
{
    public int Id { get; set; }
    public int ClassTimetableEntryId { get; set; }
    public ClassTimetableEntry ClassTimetableEntry { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
