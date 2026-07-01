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

    [Inject]
    private ILogger<ReportEntryPage> Logger { get; set; } = null!;

    [Parameter]
    public int? EntryId { get; set; }

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    [SupplyParameterFromForm(FormName = "report-entry-form", Name = "Model")]
    private ReportEntryFormModel? SubmittedEntry { get; set; }

    protected ReportEntryEditorViewModel? ViewModel { get; private set; }
    protected string? ErrorMessage { get; private set; }

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

        ErrorMessage = null;
        try
        {
            var draft = await ReportEntryService.SaveDraftAsync(form);
            ViewModel = await ReportEntryService.RefreshEditorAsync(draft);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save draft for date {Date}", form.Date);
            ErrorMessage = "Entwurf konnte nicht gespeichert werden.";
        }
    }

    protected async Task HandleSaveAsync(ReportEntryFormModel form)
    {
        if (ViewModel is null)
        {
            return;
        }

        ErrorMessage = null;
        try
        {
            var selectedDate = form.Date.Date;
            await ReportEntryService.SaveEntryAsync(form);
            ViewModel = await ReportEntryService.GetFreshEditorAsync(selectedDate);
            SubmittedEntry = null;
            Navigation.NavigateTo($"report-entries/new?date={selectedDate:yyyy-MM-dd}", replace: true);
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(ex, "Validation failed when saving report entry");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save report entry for date {Date}", form.Date);
            ErrorMessage = "Eintrag konnte nicht gespeichert werden.";
        }
    }

    protected async Task HandleDateChangedAsync(DateTime date)
    {
        var selectedDate = date.Date;
        ViewModel = await ReportEntryService.GetFreshEditorAsync(selectedDate);
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
        SubmittedEntry = null;
        Navigation.NavigateTo($"report-entries/new?date={selectedDate:yyyy-MM-dd}", replace: true);
    }

    protected async Task HandleDeleteAsync()
    {
        if (ViewModel?.Entry.Id is null)
        {
            return;
        }

        ErrorMessage = null;
        try
        {
            await ReportEntryService.DeleteEntryAsync(ViewModel.Entry.Id.Value);
            Navigation.NavigateTo("report-entries/new");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete report entry {EntryId}", ViewModel.Entry.Id.Value);
            ErrorMessage = "Eintrag konnte nicht gelöscht werden.";
        }
    }

    protected async Task HandleCreateCategoryAsync(string name)
    {
        if (ViewModel is null || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        ErrorMessage = null;
        try
        {
            var categoryId = await ReportEntryService.CreateCategoryAsync(name);
            ViewModel.Entry.CategoryId = categoryId;
            ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(ex, "Validation failed when creating category");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create category '{CategoryName}'", name);
            ErrorMessage = "Kategorie konnte nicht erstellt werden.";
        }
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

        if (string.IsNullOrWhiteSpace(ViewModel.Entry.Description))
        {
            ViewModel.Entry.Description = suggestion.SubjectsText;
        }

        ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
    }
}
