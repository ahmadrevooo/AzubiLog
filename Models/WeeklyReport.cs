namespace AzubiLog.Models;

public class WeeklyReport
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int CalendarWeek { get; set; }
    public double TotalHours { get; set; }
    public int Year { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ReportEntry> ReportEntries { get; set; } = new();
}
