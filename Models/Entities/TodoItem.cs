namespace AzubiLog.Models;
/// <summary>Represents a one-time task owned by an apprentice.</summary>
public sealed class TodoItem { public int Id { get; set; } public string UserId { get; set; } = string.Empty; public ApplicationUser User { get; set; } = null!; public string Title { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsCompleted { get; set; } public DateTimeOffset? DueDate { get; set; } public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow; }
