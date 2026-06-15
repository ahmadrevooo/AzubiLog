using AzubiLog.DTOs;
using AzubiLog.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AzubiLog.Components.Pages;

/// <summary>Renders the current apprentice dashboard.</summary>
public partial class Home
{
    [Inject] private IDashboardService DashboardService { get; set; } = null!;
    [Inject] private IStringLocalizer<Home> Localizer { get; set; } = null!;
    private DashboardDto? dashboard;
    private bool draftCreated;
    private decimal HoursPercent => dashboard is null ? 0 : dashboard.CompletedHours / dashboard.TargetHours * 100;
    private decimal EntriesPercent => dashboard is null ? 0 : (decimal)dashboard.CompletedEntries / dashboard.RequiredEntries * 100;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync() => dashboard = await DashboardService.GetAsync();

    private async Task CreateDraftAsync()
    {
        await DashboardService.CreateTodayDraftAsync();
        draftCreated = true;
    }
}
