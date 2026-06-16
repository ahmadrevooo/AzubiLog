using AzubiLog.Services.ReportEntries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AzubiLog.Components.Pages;

public partial class ReportEntryPage : ComponentBase
{
    [Inject]
    private IReportEntryService ReportEntryService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    public int? EntryId { get; set; }

    [SupplyParameterFromQuery(Name = "date")]
    public DateTime? Date { get; set; }

    protected ReportEntryEditorViewModel? ViewModel { get; private set; }

    protected override async Task OnParametersSetAsync()
    {
        ViewModel = await ReportEntryService.GetEditorAsync(EntryId, Date);
    }

    protected async Task HandleFieldChangedAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var draft = await ReportEntryService.SaveDraftAsync(ViewModel.Entry);
        ViewModel = await ReportEntryService.RefreshEditorAsync(draft);
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
    }

    protected async Task HandleDeleteAsync()
    {
        if (ViewModel?.Entry.Id is null)
        {
            return;
        }

        var confirmed = await JsRuntime.InvokeAsync<bool>(
            "confirm",
            Localizer["ReportEntryDeleteConfirm"].Value);
        if (!confirmed)
        {
            return;
        }

        await ReportEntryService.DeleteEntryAsync(ViewModel.Entry.Id.Value);
        Navigation.NavigateTo("report-entries/new");
    }
}
