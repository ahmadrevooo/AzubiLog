using System.Globalization;
using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class WeeklyOverviewVerticalPage : ComponentBase
{
    [Inject]
    private IReportEntryService ReportEntryService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    protected WeeklyOverviewViewModel? Overview { get; private set; }
    protected DateTime SelectedDate => (Date ?? DateTime.Today).Date;
    protected string SelectedDateString => SelectedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    protected string ExportHref => $"/weekly-reports/export?date={SelectedDateString}";

    protected override async Task OnParametersSetAsync()
    {
        Overview = await ReportEntryService.GetWeeklyOverviewAsync(SelectedDate);
    }

    protected void GoToPreviousWeek()
    {
        NavigateToWeek(SelectedDate.AddDays(-7));
    }

    protected void GoToNextWeek()
    {
        NavigateToWeek(SelectedDate.AddDays(7));
    }

    protected void GoToToday()
    {
        NavigateToWeek(DateTime.Today);
    }

    protected Task HandleDateChangedAsync(ChangeEventArgs args)
    {
        if (DateTime.TryParse(args.Value?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedDate))
        {
            NavigateToWeek(selectedDate);
        }

        return Task.CompletedTask;
    }

    private void NavigateToWeek(DateTime date)
    {
        Navigation.NavigateTo($"/weekly-reports/vertical?date={date:yyyy-MM-dd}");
    }
}
