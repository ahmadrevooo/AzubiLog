using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.DashboardNotes;

public class DashboardNoteFormModel
{
    [Required(ErrorMessage = "Bitte gib eine Notiz ein.")]
    [StringLength(1_000, ErrorMessage = "Die Notiz darf maximal 1000 Zeichen lang sein.")]
    public string Content { get; set; } = string.Empty;
}
