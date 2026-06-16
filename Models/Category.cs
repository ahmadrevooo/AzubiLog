namespace AzubiLog.Models;

public class Category
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public int SortOrder { get; set; }

    public List<ReportEntry> ReportEntries { get; set; } = new();
}
