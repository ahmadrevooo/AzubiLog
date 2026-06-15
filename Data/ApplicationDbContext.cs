using AzubiLog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Data;

/// <summary>Provides the Entity Framework Core persistence boundary for AzubiLog.</summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Wochenbericht> WeeklyReports => Set<Wochenbericht>();
    public DbSet<BerichtEintrag> ReportEntries => Set<BerichtEintrag>();
    public DbSet<Kategorie> Categories => Set<Kategorie>();
    public DbSet<Ausbilder> Trainers => Set<Ausbilder>();
    public DbSet<ActivityTemplate> ActivityTemplates => Set<ActivityTemplate>();
    public DbSet<VacationEntry> VacationEntries => Set<VacationEntry>();
    public DbSet<SickLeaveEntry> SickLeaveEntries => Set<SickLeaveEntry>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Wochenbericht>().HasIndex(report => new { report.UserId, report.Year, report.CalendarWeek }).IsUnique();
        builder.Entity<Kategorie>().HasIndex(category => new { category.UserId, category.Name }).IsUnique();
        builder.Entity<BerichtEintrag>().Property(entry => entry.WorkingHours).HasPrecision(5, 2);
        builder.Entity<ApplicationUser>().Property(user => user.WeeklyTargetHours).HasPrecision(5, 2);
        builder.Entity<VacationEntry>().Property(entry => entry.VacationDays).HasPrecision(5, 2);
        builder.Entity<BerichtEintrag>().HasOne(entry => entry.Trainer).WithMany(trainer => trainer.ReportEntries).HasForeignKey(entry => entry.TrainerId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<BerichtEintrag>().HasOne(entry => entry.Category).WithMany(category => category.ReportEntries).HasForeignKey(entry => entry.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<BerichtEintrag>().HasOne(entry => entry.WeeklyReport).WithMany(report => report.ReportEntries).HasForeignKey(entry => entry.WeeklyReportId);
        builder.Entity<ActivityTemplate>().HasOne(template => template.Category).WithMany(category => category.ActivityTemplates).HasForeignKey(template => template.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
