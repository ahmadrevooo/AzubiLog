namespace AzubiLog.Services.Dashboard;

public sealed record DashboardMetric(
    string LabelKey,
    string Value,
    string Detail,
    string AccentCssClass);
