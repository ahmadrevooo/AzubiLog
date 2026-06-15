namespace AzubiLog.Models;

/// <summary>Represents a trainer who may be assigned to report entries.</summary>
public class Ausbilder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public ICollection<BerichtEintrag> ReportEntries { get; set; } = new List<BerichtEintrag>();
}
