using AzubiLog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityUserContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ReportEntry> ReportEntries => Set<ReportEntry>();
    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureApplicationUser(builder);
        ConfigureCategory(builder);
        ConfigureTrainer(builder);
        ConfigureWeeklyReport(builder);
        ConfigureReportEntry(builder);
        ConfigureTodoItem(builder);
    }

    private static void ConfigureApplicationUser(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FirstName)
                .HasMaxLength(100);

            entity.Property(user => user.LastName)
                .HasMaxLength(100);

            entity.Property(user => user.CompanyName)
                .HasMaxLength(150);

            entity.Property(user => user.School)
                .HasMaxLength(150);

            entity.Property(user => user.ClassName)
                .HasMaxLength(80);

            entity.Property(user => user.TrainingOccupation)
                .HasMaxLength(150);

            entity.Property(user => user.TrainerName)
                .HasMaxLength(150);

            entity.Property(user => user.Subjects)
                .HasMaxLength(500);

            entity.Property(user => user.TrainingYear)
                .HasDefaultValue(1);

            entity.Property(user => user.WeeklyTargetHours)
                .HasDefaultValue(40d);

            entity.Property(user => user.AnnualVacationDays)
                .HasDefaultValue(30);
        });
    }

    private static void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.Property(category => category.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(category => category.ColorHex)
                .HasMaxLength(7)
                .IsRequired();

            entity.HasIndex(category => new { category.UserId, category.Name })
                .IsUnique();

            entity.HasOne(category => category.User)
                .WithMany(user => user.Categories)
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTrainer(ModelBuilder builder)
    {
        builder.Entity<Trainer>(entity =>
        {
            entity.ToTable("Trainers");

            entity.Property(trainer => trainer.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(trainer => trainer.Email)
                .HasMaxLength(256);

            entity.Property(trainer => trainer.Department)
                .HasMaxLength(120);

            entity.HasIndex(trainer => trainer.Email);
        });
    }

    private static void ConfigureWeeklyReport(ModelBuilder builder)
    {
        builder.Entity<WeeklyReport>(entity =>
        {
            entity.ToTable("WeeklyReports");

            entity.Property(report => report.Status)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(report => report.Comment)
                .HasMaxLength(2_000);

            entity.HasIndex(report => new { report.UserId, report.Year, report.CalendarWeek })
                .IsUnique();

            entity.HasOne(report => report.User)
                .WithMany(user => user.WeeklyReports)
                .HasForeignKey(report => report.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureReportEntry(ModelBuilder builder)
    {
        builder.Entity<ReportEntry>(entity =>
        {
            entity.ToTable("ReportEntries");

            entity.Property(entry => entry.DayType)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(entry => entry.OrderNumber)
                .HasMaxLength(80);

            entity.Property(entry => entry.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(entry => entry.Description)
                .HasMaxLength(4_000)
                .IsRequired();

            entity.Property(entry => entry.Note)
                .HasMaxLength(2_000);

            entity.Property(entry => entry.Subject)
                .HasMaxLength(150);

            entity.Property(entry => entry.Duration)
                .HasPrecision(5, 2);

            entity.Property(entry => entry.Status)
                .HasMaxLength(40)
                .IsRequired();

            entity.HasIndex(entry => new { entry.UserId, entry.Date });
            entity.HasIndex(entry => entry.WeeklyReportId);

            entity.HasOne(entry => entry.User)
                .WithMany(user => user.ReportEntries)
                .HasForeignKey(entry => entry.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(entry => entry.WeeklyReport)
                .WithMany(report => report.ReportEntries)
                .HasForeignKey(entry => entry.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(entry => entry.Trainer)
                .WithMany(trainer => trainer.ReportEntries)
                .HasForeignKey(entry => entry.TrainerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(entry => entry.Category)
                .WithMany(category => category.ReportEntries)
                .HasForeignKey(entry => entry.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureTodoItem(ModelBuilder builder)
    {
        builder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("Todos");

            entity.Property(todo => todo.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(todo => todo.Description)
                .HasMaxLength(2_000);

            entity.HasIndex(todo => new { todo.UserId, todo.IsCompleted, todo.DueDate });

            entity.HasOne(todo => todo.User)
                .WithMany(user => user.Todos)
                .HasForeignKey(todo => todo.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
