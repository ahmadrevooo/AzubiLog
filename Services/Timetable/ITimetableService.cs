using AzubiLog.Models;

namespace AzubiLog.Services.Timetable;

public interface ITimetableService
{
    Task<List<ClassTimetableEntry>> GetClassTimetableAsync(
        string school,
        string className,
        CancellationToken cancellationToken = default);

    Task SaveClassTimetableAsync(
        string userId,
        string school,
        string className,
        DayOfWeek dayOfWeek,
        string subjectsText,
        CancellationToken cancellationToken = default);
}
