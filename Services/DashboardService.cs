using AzubiLog.DTOs;
using AzubiLog.Interfaces;
using System.Globalization;

namespace AzubiLog.Services;

/// <summary>Provides dashboard data until the persistent report-entry workflow is introduced.</summary>
public sealed class DashboardService(ILogger<DashboardService> logger) : IDashboardService
{
    /// <inheritdoc />
    public Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var entries = Enumerable.Range(0, 5).Select(index => new DashboardEntryDto(
            monday.AddDays(index), index switch { 0 => "ApiIntegration", 1 => "DatabaseDesign", 2 => "BusinessProcesses", _ => "NoEntry" },
            index == 2 ? "VocationalSchool" : index < 2 ? "Development" : "AddEntry", index < 3 ? 8 : null,
            index < 3 ? DashboardEntryStatus.Complete : DashboardEntryStatus.Missing)).ToArray();
        var result = new DashboardDto(today, "Lena", 24, 40, 3, 5, 18, 30, ISOWeek.GetWeekOfYear(today.ToDateTime(TimeOnly.MinValue)), monday, monday.AddDays(4), entries);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task CreateTodayDraftAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("A dashboard draft entry was created.");
        return Task.CompletedTask;
    }
}
