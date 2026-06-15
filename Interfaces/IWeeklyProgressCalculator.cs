using AzubiLog.DTOs;
using AzubiLog.Models;

namespace AzubiLog.Interfaces;

/// <summary>Calculates weekly progress without persisting derived values.</summary>
public interface IWeeklyProgressCalculator
{
    /// <summary>Calculates progress from the user's target and assigned report entries.</summary>
    /// <param name="weeklyTargetHours">The expected hours for the week.</param>
    /// <param name="entries">Entries assigned to the week.</param>
    /// <returns>Dynamically calculated weekly progress.</returns>
    WeeklyProgressDto Calculate(decimal weeklyTargetHours, IEnumerable<BerichtEintrag> entries);
}
