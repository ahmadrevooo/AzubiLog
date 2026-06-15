namespace AzubiLog.Models;

/// <summary>Represents one reporting week owned by an apprentice.</summary>
public class Wochenbericht
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int CalendarWeek { get; set; }
    public int Year { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<BerichtEintrag> ReportEntries { get; set; } = new List<BerichtEintrag>();
}
