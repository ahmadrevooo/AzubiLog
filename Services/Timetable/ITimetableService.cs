using AzubiLog.Models;

namespace AzubiLog.Services.Timetable;

public interface ITimetableService
{
    Task<List<ClassTimetableEntry>> GetClassTimetableAsync(
        string school,
        string className,
        CancellationToken cancellationToken = default);

    Task<ClassTimetableEntry?> GetClassTimetableForDayAsync(
        string school,
        string className,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default);

    Task SaveClassTimetableAsync(
        string userId,
        string school,
        string className,
        DayOfWeek dayOfWeek,
        string subjectsText,
        CancellationToken cancellationToken = default);

    Task DeleteClassTimetableEntryAsync(
        string userId,
        int entryId,
        CancellationToken cancellationToken = default);

    Task<List<TimetableCancellation>> GetCancellationsForDateAsync(
        string school,
        string className,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task AddCancellationAsync(
        string userId,
        int classTimetableEntryId,
        DateTime date,
        string? reason,
        CancellationToken cancellationToken = default);

    Task RemoveCancellationAsync(
        string userId,
        int cancellationId,
        CancellationToken cancellationToken = default);

    Task ShareTimetableAsync(
        string userId,
        string targetSchool,
        string targetClassName,
        string sourceSchool,
        string sourceClassName,
        CancellationToken cancellationToken = default);
}
