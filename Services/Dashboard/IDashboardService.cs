namespace AzubiLog.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
}
