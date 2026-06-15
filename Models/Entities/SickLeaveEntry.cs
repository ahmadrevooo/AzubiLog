namespace AzubiLog.Models;
/// <summary>Represents a sickness period owned by an apprentice.</summary>
public sealed class SickLeaveEntry { public int Id { get; set; } public string UserId { get; set; } = string.Empty; public ApplicationUser User { get; set; } = null!; public DateOnly StartDate { get; set; } public DateOnly EndDate { get; set; } public string Notes { get; set; } = string.Empty; public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow; }
