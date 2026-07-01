using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.Account;

public sealed class ChangePasswordFormModel
{
    [Required(ErrorMessage = "Aktuelles Passwort ist erforderlich.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Neues Passwort ist erforderlich.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte das neue Passwort bestätigen.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Die Passwörter stimmen nicht überein.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
