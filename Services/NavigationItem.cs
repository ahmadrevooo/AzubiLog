namespace AzubiLog.Services;

public sealed record NavigationItem(
    string ResourceKey,
    string Href,
    string IconCssClass,
    bool IsEnabled);
