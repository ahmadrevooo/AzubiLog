using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AzubiLog.Services.Timetable;

public sealed class TimetableService(ApplicationDbContext dbContext) : ITimetableService
{
    public string GenerateShareCode(string school, string className)
    {
        var normalizedKey = BuildNormalizedClassKey(school, className);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedKey));
        var value = BitConverter.ToUInt64(hashBytes, 0);
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var buffer = new char[6];

        for (var index = buffer.Length - 1; index >= 0; index--)
        {
            buffer[index] = alphabet[(int)(value % 36)];
            value /= 36;
        }

        return new string(buffer);
    }

    public async Task<(string School, string ClassName)?> ResolveShareCodeAsync(
        string shareCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = shareCode.Trim().ToUpperInvariant();
        if (normalizedCode.Length != 6)
        {
            return null;
        }

        var classPairs = await dbContext.ClassTimetableEntries
            .Select(entry => new { entry.School, entry.ClassName })
            .Distinct()
            .ToListAsync(cancellationToken);

        var match = classPairs.FirstOrDefault(pair =>
            string.Equals(
                GenerateShareCode(pair.School, pair.ClassName),
                normalizedCode,
                StringComparison.OrdinalIgnoreCase));

        return match is null ? null : (match.School, match.ClassName);
    }

    public async Task<List<ClassTimetableEntry>> GetClassTimetableAsync(
        string school,
        string className,
        CancellationToken cancellationToken = default)
    {
        var (normalizedSchool, normalizedClass) = TimetableNormalizer.Normalize(school, className);

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
        var (normalizedSchool, normalizedClass) = TimetableNormalizer.Normalize(school, className);

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
        var (normalizedSchool, normalizedClass) = TimetableNormalizer.Normalize(school, className);

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
        var (normalizedSchool, normalizedClass) = TimetableNormalizer.Normalize(school, className);
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

    public async Task ShareTimetableAsync(
        string userId,
        string targetSchool,
        string targetClassName,
        string sourceSchool,
        string sourceClassName,
        CancellationToken cancellationToken = default)
    {
        var (normalizedTargetSchool, normalizedTargetClass) = TimetableNormalizer.Normalize(targetSchool, targetClassName);
        var (normalizedSourceSchool, normalizedSourceClass) = TimetableNormalizer.Normalize(sourceSchool, sourceClassName);

        var sourceEntries = await dbContext.ClassTimetableEntries
            .Include(entry => entry.Cancellations)
            .Where(entry =>
                entry.School.ToUpper() == normalizedSourceSchool &&
                entry.ClassName.ToUpper() == normalizedSourceClass)
            .OrderBy(entry => entry.DayOfWeek)
            .ToListAsync(cancellationToken);

        if (sourceEntries.Count == 0)
        {
            throw new InvalidOperationException("Source timetable not found.");
        }

        var targetEntries = await dbContext.ClassTimetableEntries
            .Include(entry => entry.Cancellations)
            .Where(entry =>
                entry.School.ToUpper() == normalizedTargetSchool &&
                entry.ClassName.ToUpper() == normalizedTargetClass)
            .ToListAsync(cancellationToken);

        dbContext.TimetableCancellations.RemoveRange(targetEntries.SelectMany(entry => entry.Cancellations));
        dbContext.ClassTimetableEntries.RemoveRange(targetEntries);

        var clonedEntries = sourceEntries
            .Select(entry => new ClassTimetableEntry
            {
                School = targetSchool.Trim(),
                ClassName = targetClassName.Trim(),
                DayOfWeek = entry.DayOfWeek,
                SubjectsText = entry.SubjectsText,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Cancellations = entry.Cancellations
                    .Select(cancellation => new TimetableCancellation
                    {
                        Date = cancellation.Date,
                        Reason = cancellation.Reason,
                        CreatedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList()
            })
            .ToList();

        dbContext.ClassTimetableEntries.AddRange(clonedEntries);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildNormalizedClassKey(string school, string className)
        => $"{TimetableNormalizer.NormalizeSchool(school)}|{TimetableNormalizer.NormalizeClassName(className)}";
}
