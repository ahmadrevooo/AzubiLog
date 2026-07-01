using System.ComponentModel.DataAnnotations;

namespace AzubiLog.Services.Profile;

public sealed class ApprenticeProfileFormModel
{
    [Required(ErrorMessage = "Bitte gib deinen Vornamen ein.")]
    [StringLength(100, ErrorMessage = "Der Vorname darf maximal 100 Zeichen lang sein.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib deinen Nachnamen ein.")]
    [StringLength(100, ErrorMessage = "Der Nachname darf maximal 100 Zeichen lang sein.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib den Firmennamen ein.")]
    [StringLength(150, ErrorMessage = "Der Firmenname darf maximal 150 Zeichen lang sein.")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib deinen Ausbildungsberuf ein.")]
    [StringLength(150, ErrorMessage = "Der Ausbildungsberuf darf maximal 150 Zeichen lang sein.")]
    public string TrainingOccupation { get; set; } = string.Empty;

    [Range(1, 4, ErrorMessage = "Das Ausbildungsjahr muss zwischen 1 und 4 liegen.")]
    public int TrainingYear { get; set; } = 1;

    [Required(ErrorMessage = "Bitte gib den Namen deines Ausbilders ein.")]
    [StringLength(150, ErrorMessage = "Der Ausbildername darf maximal 150 Zeichen lang sein.")]
    public string TrainerName { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Der Schulname darf maximal 150 Zeichen lang sein.")]
    public string School { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "Die Klasse darf maximal 80 Zeichen lang sein.")]
    public string ClassName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Die Fächer dürfen maximal 500 Zeichen lang sein.")]
    public string Subjects { get; set; } = string.Empty;

    [RegularExpression(@"^#([0-9A-Fa-f]{6})$", ErrorMessage = "Bitte einen gültigen Hex-Farbcode eingeben (z.B. #2563eb).")]
    public string PdfAccentColor { get; set; } = "#2563eb";

    [Range(1, 80, ErrorMessage = "Die wöchentliche Sollarbeitszeit muss zwischen 1 und 80 Stunden liegen.")]
    public double WeeklyTargetHours { get; set; } = 40;

    [Range(0, 60, ErrorMessage = "Die Urlaubstage müssen zwischen 0 und 60 liegen.")]
    public int AnnualVacationDays { get; set; } = 30;

    public List<SchoolScheduleDayFormModel> SchoolScheduleDays { get; set; } = [];
    public List<TrainerFormModel> Trainers { get; set; } = [];
}

public sealed class SchoolScheduleDayFormModel
{
    public DayOfWeek DayOfWeek { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsSelected { get; set; }

    [StringLength(1_000, ErrorMessage = "Die Fächer dürfen maximal 1000 Zeichen lang sein.")]
    public string SubjectsText { get; set; } = string.Empty;
}

public sealed class TrainerFormModel
{
    public int? Id { get; set; }

    [StringLength(150, ErrorMessage = "Der Name darf maximal 150 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    [RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Bitte gib eine gültige E-Mail-Adresse ein.")]
    [StringLength(256, ErrorMessage = "Die E-Mail-Adresse darf maximal 256 Zeichen lang sein.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(120, ErrorMessage = "Die Abteilung darf maximal 120 Zeichen lang sein.")]
    public string Department { get; set; } = string.Empty;
}
