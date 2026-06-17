namespace AzubiLog.Models;

public class Trainer
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    public List<ReportEntry> ReportEntries { get; set; } = new();
}
