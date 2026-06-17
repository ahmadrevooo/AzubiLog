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

    protected string GetWelcomeHeading()
    {
        if (string.IsNullOrWhiteSpace(ViewModel?.ApprenticeName))
        {
            return Localizer["DashboardWelcome"];
        }

        return string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            Localizer["DashboardWelcomeNamed"],
            ViewModel.ApprenticeName);
    }
}
