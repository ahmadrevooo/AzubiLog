using System.ComponentModel.DataAnnotations;
using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class ReportEntryPage : ComponentBase
{
    [Inject]
    private IReportEntryService ReportEntryService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public int? EntryId { get; set; }

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    [SupplyParameterFromForm(FormName = "report-entry-form", Name = "Model")]
    private ReportEntryFormModel? SubmittedEntry { get; set; }

    protected ReportEntryEditorViewModel? ViewModel { get; private set; }

    protected override async Task OnParametersSetAsync()
    {
        if (SubmittedEntry is not null)
        {
            ViewModel = await ReportEntryService.RefreshEditorAsync(SubmittedEntry);
            return;
        }

        if (EntryId is null
            && Date.HasValue
            && ViewModel?.Entry.Date.Date == Date.Value.Date)
        {
            return;
        }

        ViewModel = await ReportEntryService.GetEditorAsync(EntryId, Date);
        AutoApplySchoolDay();
    }

    private void AutoApplySchoolDay()
    {
        if (ViewModel?.SchoolDaySuggestion is not { IsSchoolDay: true } suggestion)
            return;

        if (ViewModel.Entry.Id.HasValue)
            return;

        ViewModel.Entry.IsVocationalSchoolDay = true;

        if (suggestion.VocationalSchoolCategoryId is int categoryId
            && ViewModel.Entry.CategoryId is null)
        {
            ViewModel.Entry.CategoryId = categoryId;
        }

        if (!ViewModel.Entry.IsOrderNumberOverridden)
        {
            ViewModel.Entry.OrderNumber = "SCHULE";
        }
    }

    protected async Task HandleFieldChangedAsync(ReportEntryFormModel form)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel = await ReportEntryService.RefreshEditorAsync(form);
    }

    protected async Task HandleDraftAsync(ReportEntryFormModel form)
    {
        if (ViewModel is null)
        {
            return;
        }

        var draft = await ReportEntryService.SaveDraftAsync(form);
        ViewModel = await ReportEntryService.RefreshEditorAsync(draft);
    }

    protected string? SaveErrorMessage { get; private set; }

    protected async Task HandleSaveAsync(ReportEntryFormModel form)
    {
        if (ViewModel is null)
        {
            return;
        }

        SaveErrorMessage = null;

        try
        {
            var selectedDate = form.Date.Date;
            await ReportEntryService.SaveEntryAsync(form);
            ViewModel = await ReportEntryService.GetFreshEditorAsync(selectedDate);
            AutoApplySchoolDay();
            SubmittedEntry = null;
            Navigation.NavigateTo($"report-entries/new?date={selectedDate:yyyy-MM-dd}", replace: true);
        }
        catch (ValidationException ex)
        {
            SaveErrorMessage = ex.Message;
        }
    }

    protected async Task HandleDateChangedAsync(DateTime date)
    {
        var selectedDate = date.Date;
        ViewModel = await ReportEntryService.GetFreshEditorAsync(selectedDate);
        AutoApplySchoolDay();
        SubmittedEntry = null;
        Navigation.NavigateTo($"report-entries/new?date={selectedDate:yyyy-MM-dd}", replace: true);
    }

    protected async Task HandleNewEntryAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var selectedDate = ViewModel.Entry.Date.Date;
        ViewModel = await ReportEntryService.GetFreshEditorAsync(selectedDate);
        AutoApplySchoolDay();
        SubmittedEntry = null;
        Navigation.NavigateTo($"report-entries/new?date={selectedDate:yyyy-MM-dd}", replace: true);
    }

    protected async Task HandleDeleteAsync()
    {
        if (ViewModel?.Entry.Id is null)
        {
            return;
        }

        await ReportEntryService.DeleteEntryAsync(ViewModel.Entry.Id.Value);
        Navigation.NavigateTo("report-entries/new");
    }

    protected async Task HandleCreateCategoryAsync(string name)
    {
        if (ViewModel is null || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var categoryId = await ReportEntryService.CreateCategoryAsync(name);
        ViewModel.Entry.CategoryId = categoryId;
        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }

    protected async Task HandleApplySchoolDayAsync()
    {
        if (ViewModel?.SchoolDaySuggestion is not { IsSchoolDay: true } suggestion)
        {
            return;
        }

        ViewModel.Entry.IsVocationalSchoolDay = true;

        if (suggestion.VocationalSchoolCategoryId is int categoryId)
        {
            ViewModel.Entry.CategoryId = categoryId;
        }

        if (!ViewModel.Entry.IsOrderNumberOverridden)
        {
            ViewModel.Entry.OrderNumber = "SCHULE";
        }

        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }
}
