namespace AzubiLog.DTOs;

/// <summary>Contains the information required to render the apprentice dashboard.</summary>
public sealed record DashboardDto(
    DateOnly Today,
    string ApprenticeName,
    decimal CompletedHours,
    decimal TargetHours,
    int CompletedEntries,
    int RequiredEntries,
    int RemainingVacationDays,
    int AnnualVacationDays,
    int CalendarWeek,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    IReadOnlyList<DashboardEntryDto> Entries);

/// <summary>Represents a daily entry displayed in the current-week overview.</summary>
public sealed record DashboardEntryDto(DateOnly Date, string TitleKey, string CategoryKey, decimal? Hours, DashboardEntryStatus Status);

/// <summary>Describes the completion state of a dashboard entry.</summary>
public enum DashboardEntryStatus { Missing, Complete }
