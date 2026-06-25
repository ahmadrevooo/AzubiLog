using Microsoft.AspNetCore.Identity;

namespace AzubiLog.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.Azubi;
    public string CompanyName { get; set; } = string.Empty;
    public string School { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string TrainingOccupation { get; set; } = string.Empty;
    public int TrainingYear { get; set; } = 1;
    public string TrainerName { get; set; } = string.Empty;
    public string Subjects { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public double WeeklyTargetHours { get; set; } = 40;
    public int AnnualVacationDays { get; set; } = 30;

    public List<WeeklyReport> WeeklyReports { get; set; } = new();
    public List<ReportEntry> ReportEntries { get; set; } = new();
    public List<SchoolScheduleDay> SchoolScheduleDays { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Trainer> Trainers { get; set; } = new();
    public List<TodoItem> Todos { get; set; } = new();
    public List<ClassTimetableEntry> ClassTimetableEntries { get; set; } = new();
}
