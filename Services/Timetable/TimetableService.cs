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
            .Include(entry => entry.Cancellations)
            .Where(entry =>
                entry.School.ToUpper() == normalizedSchool &&
                entry.ClassName.ToUpper() == normalizedClass)
            .OrderBy(entry => entry.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassTimetableEntry?> GetClassTimetableForDayAsync(
        string school,
        string className,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        var normalizedSchool = school.Trim().ToUpperInvariant();
        var normalizedClass = className.Trim().ToUpperInvariant();

        return await dbContext.ClassTimetableEntries
            .Include(entry => entry.Cancellations)
            .FirstOrDefaultAsync(entry =>
                entry.School.ToUpper() == normalizedSchool &&
                entry.ClassName.ToUpper() == normalizedClass &&
                entry.DayOfWeek == dayOfWeek,
                cancellationToken);
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

    public async Task DeleteClassTimetableEntryAsync(
        string userId,
        int entryId,
        CancellationToken cancellationToken = default)
    {
        var entry = await dbContext.ClassTimetableEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.CreatedByUserId == userId,
                cancellationToken);

        if (entry is not null)
        {
            dbContext.ClassTimetableEntries.Remove(entry);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<TimetableCancellation>> GetCancellationsForDateAsync(
        string school,
        string className,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var normalizedSchool = school.Trim().ToUpperInvariant();
        var normalizedClass = className.Trim().ToUpperInvariant();
        var dateOnly = date.Date;

        return await dbContext.TimetableCancellations
            .Include(c => c.ClassTimetableEntry)
            .Where(c =>
                c.ClassTimetableEntry.School.ToUpper() == normalizedSchool &&
                c.ClassTimetableEntry.ClassName.ToUpper() == normalizedClass &&
                c.Date == dateOnly)
            .ToListAsync(cancellationToken);
    }

    public async Task AddCancellationAsync(
        string userId,
        int classTimetableEntryId,
        DateTime date,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;

        var exists = await dbContext.TimetableCancellations
            .AnyAsync(c =>
                c.ClassTimetableEntryId == classTimetableEntryId &&
                c.Date == dateOnly,
                cancellationToken);

        if (exists) return;

        dbContext.TimetableCancellations.Add(new TimetableCancellation
        {
            ClassTimetableEntryId = classTimetableEntryId,
            Date = dateOnly,
            Reason = reason?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveCancellationAsync(
        string userId,
        int cancellationId,
        CancellationToken cancellationToken = default)
    {
        var cancellation = await dbContext.TimetableCancellations
            .FirstOrDefaultAsync(c => c.Id == cancellationId,
                cancellationToken);

        if (cancellation is not null)
        {
            dbContext.TimetableCancellations.Remove(cancellation);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
