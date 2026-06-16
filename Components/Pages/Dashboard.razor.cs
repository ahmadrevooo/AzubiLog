using AzubiLog.Services.Dashboard;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class Dashboard : ComponentBase
{
    [Inject]
    private IDashboardService DashboardService { get; set; } = null!;

    protected DashboardViewModel? ViewModel { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        ViewModel = await DashboardService.GetDashboardAsync();
    }
}
