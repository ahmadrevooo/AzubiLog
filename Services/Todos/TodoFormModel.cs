using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.Todos;

public sealed class TodoFormModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2_000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
}
