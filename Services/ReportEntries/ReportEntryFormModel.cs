using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.ReportEntries;

public class ReportEntryFormModel
{
    public int? Id { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required]
    public int? CategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4_000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(2_000)]
    public string Notes { get; set; } = string.Empty;

    [StringLength(80)]
    public string OrderNumber { get; set; } = string.Empty;

    public bool IsOrderNumberOverridden { get; set; }

    public int? TrainerId { get; set; }

    public string StartTime { get; set; } = "08:00";

    public string EndTime { get; set; } = "16:00";

    public bool IsVocationalSchoolDay { get; set; }

    [StringLength(150)]
    public string? Subject { get; set; }

    public bool IsDraft { get; set; } = true;
}
