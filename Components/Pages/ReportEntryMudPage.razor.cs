using System.Globalization;
using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class ReportEntryMudPage : ComponentBase
{
    [Inject]
    private IReportEntryService ReportEntryService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public int? EntryId { get; set; }

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    protected ReportEntryEditorViewModel? ViewModel { get; private set; }

    protected TimeSpan? StartTimeValue => ParseTime(ViewModel?.Entry.StartTime);
    protected TimeSpan? EndTimeValue => ParseTime(ViewModel?.Entry.EndTime);
    protected double WeeklyProgressPercentage
    {
        get
        {
            if (ViewModel?.WeeklyOverview.WeeklyTargetHours is null or <= 0)
            {
                return 0;
            }

            var percentage = ViewModel.WeeklyOverview.RecordedHours / ViewModel.WeeklyOverview.WeeklyTargetHours * 100m;
            return (double)Math.Clamp(percentage, 0m, 100m);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        ViewModel = await ReportEntryService.GetEditorAsync(EntryId, Date);
    }

    protected async Task HandleCategoryChangedAsync(int? categoryId)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Entry.CategoryId = categoryId;
        ViewModel.Entry.IsOrderNumberOverridden = false;
        ViewModel.Entry.OrderNumber = string.Empty;
        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected async Task HandleTrainerChangedAsync(int? trainerId)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Entry.TrainerId = trainerId;
        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected async Task HandleDateChangedAsync(DateTime? date)
    {
        if (ViewModel is null || !date.HasValue)
        {
            return;
        }

        ViewModel.Entry.Date = date.Value.Date;
        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected async Task HandleStartTimeChangedAsync(TimeSpan? time)
    {
        if (ViewModel is null || !time.HasValue)
        {
            return;
        }

        ViewModel.Entry.StartTime = FormatTime(time.Value);
        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected async Task HandleEndTimeChangedAsync(TimeSpan? time)
    {
        if (ViewModel is null || !time.HasValue)
        {
            return;
        }

        ViewModel.Entry.EndTime = FormatTime(time.Value);
        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected Task MoveToToday()
    {
        return HandleDateChangedAsync(DateTime.Today);
    }

    protected async Task ApplySchoolDayTemplateAsync()
    {
        if (ViewModel?.SchoolDaySuggestion is null)
        {
            return;
        }

        ViewModel.Entry.IsVocationalSchoolDay = true;
        if (ViewModel.SchoolDaySuggestion.VocationalSchoolCategoryId is int categoryId)
        {
            ViewModel.Entry.CategoryId = categoryId;
        }

        if (!ViewModel.Entry.IsOrderNumberOverridden)
        {
            ViewModel.Entry.OrderNumber = "SCHULE";
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Entry.Description))
        {
            ViewModel.Entry.Description = ViewModel.SchoolDaySuggestion.SubjectsText;
        }

        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected async Task HandleDraftAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var draft = await ReportEntryService.SaveDraftAsync(ViewModel.Entry);
        ViewModel = await ReportEntryService.RefreshEditorAsync(draft);
    }

    protected async Task HandleSaveAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var entryId = await ReportEntryService.SaveEntryAsync(ViewModel.Entry);
        ViewModel = await ReportEntryService.GetEditorAsync(entryId, null);
        Navigation.NavigateTo($"report-entries/mud/{entryId}", replace: true);
    }

    protected async Task HandleDeleteAsync()
    {
        if (ViewModel?.Entry.Id is null)
        {
            return;
        }

        await ReportEntryService.DeleteEntryAsync(ViewModel.Entry.Id.Value);
        Navigation.NavigateTo("report-entries/mud");
    }

    private static TimeSpan? ParseTime(string? value)
    {
        return TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var time) ? time : null;
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
    }
}
