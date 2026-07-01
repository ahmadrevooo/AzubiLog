using AzubiLog.Data;
using AzubiLog.Models;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.Timetable;

public sealed class TimetableService(ApplicationDbContext dbContext) : ITimetableService
{
    public async Task<List<ClassTimetableEntry>> GetClassTimetableAsync(
        string school,
        string className,
        CancellationToken cancellationToken = default)
    {
        var normalizedSchool = school.Trim().ToUpperInvariant();
        var normalizedClass = className.Trim().ToUpperInvariant();

        return await dbContext.ClassTimetableEntries
            .Where(entry =>
                entry.School.ToUpper() == normalizedSchool &&
                entry.ClassName.ToUpper() == normalizedClass)
            .OrderBy(entry => entry.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveClassTimetableAsync(
        string userId,
        string school,
        string className,
        DayOfWeek dayOfWeek,
        string subjectsText,
        CancellationToken cancellationToken = default)
    {
        var normalizedSchool = school.Trim().ToUpperInvariant();
        var normalizedClass = className.Trim().ToUpperInvariant();

        var existing = await dbContext.ClassTimetableEntries
            .FirstOrDefaultAsync(entry =>
                entry.School.ToUpper() == normalizedSchool &&
                entry.ClassName.ToUpper() == normalizedClass &&
                entry.DayOfWeek == dayOfWeek,
                cancellationToken);

        if (string.IsNullOrWhiteSpace(subjectsText))
        {
            if (existing is not null)
            {
                dbContext.ClassTimetableEntries.Remove(existing);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        if (existing is not null)
        {
            existing.SubjectsText = subjectsText.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            dbContext.ClassTimetableEntries.Add(new ClassTimetableEntry
            {
                School = school.Trim(),
                ClassName = className.Trim(),
                DayOfWeek = dayOfWeek,
                SubjectsText = subjectsText.Trim(),
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
