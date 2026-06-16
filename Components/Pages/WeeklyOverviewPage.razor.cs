using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class WeeklyOverviewPage : ComponentBase
{
    [Inject]
    private IReportEntryService ReportEntryService { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    protected WeeklyOverviewViewModel? Overview { get; private set; }
    protected string ExportHref => $"/weekly-reports/export?date={(Date ?? DateTime.Today):yyyy-MM-dd}";

    protected override async Task OnParametersSetAsync()
    {
        Overview = await ReportEntryService.GetWeeklyOverviewAsync(Date ?? DateTime.Today);
    }
}
