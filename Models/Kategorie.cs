namespace AzubiLog.Models;

/// <summary>Groups a user's report entries.</summary>
public class Kategorie
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public int SortOrder { get; set; }
    public ICollection<BerichtEintrag> ReportEntries { get; set; } = new List<BerichtEintrag>();
    public ICollection<ActivityTemplate> ActivityTemplates { get; set; } = new List<ActivityTemplate>();
}
