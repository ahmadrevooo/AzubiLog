namespace AzubiLog.Services.Dashboard;

public sealed record DashboardQuickAction(
    string LabelKey,
    string DescriptionKey,
    string Href,
    string Icon,
    string AccentCssClass,
    bool IsEnabled);
