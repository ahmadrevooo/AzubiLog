using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.Account;

public sealed class AccountOverviewFormModel
{
    [Required(ErrorMessage = "Vorname ist erforderlich.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nachname ist erforderlich.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-Mail ist erforderlich.")]
    [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 80, ErrorMessage = "Bitte eine gültige Stundenzahl eingeben.")]
    public double WeeklyTargetHours { get; set; } = 40;

    [Range(0, 60, ErrorMessage = "Bitte eine gültige Anzahl an Urlaubstagen eingeben.")]
    public int AnnualVacationDays { get; set; } = 30;

    [StringLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(150)]
    public string TrainingOccupation { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Bitte ein gültiges Ausbildungsjahr eingeben.")]
    public int TrainingYear { get; set; } = 1;
}
