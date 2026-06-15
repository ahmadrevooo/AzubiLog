using Microsoft.AspNetCore.Identity;

namespace AzubiLog.Models;

/// <summary>Represents an authenticated apprentice and their business profile.</summary>
public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string School { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public int TrainingYear { get; set; } = 1;
    public string TrainerName { get; set; } = string.Empty;
    public decimal WeeklyTargetHours { get; set; } = 40;
    public int? AnnualVacationDays { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Wochenbericht> WeeklyReports { get; set; } = new List<Wochenbericht>();
    public ICollection<BerichtEintrag> ReportEntries { get; set; } = new List<BerichtEintrag>();
    public ICollection<Kategorie> Categories { get; set; } = new List<Kategorie>();
    public ICollection<VacationEntry> VacationEntries { get; set; } = new List<VacationEntry>();
    public ICollection<SickLeaveEntry> SickLeaveEntries { get; set; } = new List<SickLeaveEntry>();
    public ICollection<ActivityTemplate> ActivityTemplates { get; set; } = new List<ActivityTemplate>();
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}
