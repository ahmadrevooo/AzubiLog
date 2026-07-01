using System.Globalization;
using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public abstract class WeeklyOverviewPageBase : ComponentBase
{
    [Inject]
    protected IReportEntryService ReportEntryService { get; set; } = null!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    protected WeeklyOverviewViewModel? Overview { get; private set; }
    protected DateTime SelectedDate => (Date ?? DateTime.Today).Date;
    protected string SelectedDateString => SelectedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    protected string ExportHref => $"/weekly-reports/export?date={SelectedDateString}";

    protected abstract string NavigationBasePath { get; }

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
        Navigation.NavigateTo($"{NavigationBasePath}?date={date:yyyy-MM-dd}");
    }
}
