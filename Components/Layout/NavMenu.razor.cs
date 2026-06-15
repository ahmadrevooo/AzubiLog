using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace AzubiLog.Components.Layout;

/// <summary>Renders the primary application navigation.</summary>
public partial class NavMenu
{
    [Inject] private IStringLocalizer<NavMenu> Localizer { get; set; } = null!;
}
