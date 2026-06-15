using AzubiLog.DTOs;

namespace AzubiLog.Interfaces;

/// <summary>Provides the current user's dashboard summary.</summary>
public interface IDashboardService
{
    /// <summary>Gets the dashboard summary without exposing persistence concerns to the UI.</summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The current dashboard summary.</returns>
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a recoverable draft entry for the current day.</summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task CreateTodayDraftAsync(CancellationToken cancellationToken = default);
}
