namespace AzubiLog.DTOs;

/// <summary>Contains dynamically calculated progress for one reporting week.</summary>
public sealed record WeeklyProgressDto(decimal WeeklyTargetHours, decimal RecordedHours, decimal RemainingHours, decimal ProgressPercentage);
