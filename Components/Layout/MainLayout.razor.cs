using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AzubiLog.Components.Layout;

/// <summary>Provides the shared application shell.</summary>
public partial class MainLayout
{
    [Inject] private IStringLocalizer<MainLayout> Localizer { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    private string CultureUrl(string culture) => $"/?culture={culture}&ui-culture={culture}";
}
