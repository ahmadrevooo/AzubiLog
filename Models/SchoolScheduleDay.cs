namespace AzubiLog.Models;

public class SchoolScheduleDay
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public string SubjectsText { get; set; } = string.Empty;
}
