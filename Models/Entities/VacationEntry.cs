namespace AzubiLog.Models;
/// <summary>Represents a vacation period owned by an apprentice.</summary>
public sealed class VacationEntry { public int Id { get; set; } public string UserId { get; set; } = string.Empty; public ApplicationUser User { get; set; } = null!; public DateOnly StartDate { get; set; } public DateOnly EndDate { get; set; } public decimal VacationDays { get; set; } public string Notes { get; set; } = string.Empty; public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow; }
