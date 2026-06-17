using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class ReportEntryWriterPage : ComponentBase
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

    protected override async Task OnParametersSetAsync()
    {
        ViewModel = await ReportEntryService.GetEditorAsync(EntryId, Date);
    }

    protected async Task RefreshAsync()
    {
        if (ViewModel is not null)
        {
            ViewModel = await ReportEntryService.RefreshEditorAsync(ViewModel.Entry);
        }
    }

    protected async Task HandleCategoryChangedAsync(ChangeEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Entry.CategoryId = int.TryParse(args.Value?.ToString(), out var categoryId) ? categoryId : null;
        ViewModel.Entry.IsOrderNumberOverridden = false;
        ViewModel.Entry.OrderNumber = string.Empty;
        await RefreshAsync();
    }

    protected async Task HandleTrainerChangedAsync(ChangeEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Entry.TrainerId = int.TryParse(args.Value?.ToString(), out var trainerId) ? trainerId : null;
        await RefreshAsync();
    }

    protected Task MoveToToday()
    {
        if (ViewModel is not null)
        {
            ViewModel.Entry.Date = DateTime.Today;
        }

        return RefreshAsync();
    }

    protected async Task MarkOrderNumberChangedAsync()
    {
        if (ViewModel is not null)
        {
            ViewModel.Entry.IsOrderNumberOverridden = true;
        }

        await RefreshAsync();
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

        await RefreshAsync();
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
        Navigation.NavigateTo($"report-entries/writer/{entryId}", replace: true);
    }

    protected async Task HandleDeleteAsync()
    {
        if (ViewModel?.Entry.Id is null)
        {
            return;
        }

        await ReportEntryService.DeleteEntryAsync(ViewModel.Entry.Id.Value);
        Navigation.NavigateTo("report-entries/writer");
    }
}
