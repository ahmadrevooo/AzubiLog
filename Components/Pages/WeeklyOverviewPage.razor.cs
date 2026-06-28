using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace AzubiLog.Components.Pages;

public partial class WeeklyOverviewPage : ComponentBase
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    protected override void OnInitialized()
    {
        var target = Date.HasValue
            ? string.Create(CultureInfo.InvariantCulture, $"/weekly-reports/grid?date={Date.Value:yyyy-MM-dd}")
            : "/weekly-reports/grid";

        Navigation.NavigateTo(target, replace: true);
    }
}
