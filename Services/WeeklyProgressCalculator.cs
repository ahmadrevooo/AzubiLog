using AzubiLog.DTOs;
using AzubiLog.Interfaces;
using AzubiLog.Models;

namespace AzubiLog.Services;

/// <summary>Calculates weekly progress from report entries.</summary>
public sealed class WeeklyProgressCalculator : IWeeklyProgressCalculator
{
    /// <inheritdoc />
    public WeeklyProgressDto Calculate(decimal weeklyTargetHours, IEnumerable<BerichtEintrag> entries)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(weeklyTargetHours);
        ArgumentNullException.ThrowIfNull(entries);
        var recordedHours = entries.Sum(entry => entry.WorkingHours);
        var remainingHours = Math.Max(0, weeklyTargetHours - recordedHours);
        var percentage = weeklyTargetHours == 0 ? 0 : recordedHours / weeklyTargetHours * 100;
        return new WeeklyProgressDto(weeklyTargetHours, recordedHours, remainingHours, percentage);
    }
}
