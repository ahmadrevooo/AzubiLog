namespace AzubiLog.Services.Shared;

public static class FormatHelpers
{
    public static string FormatApprenticeName(string firstName, string lastName, string? fallback = null)
    {
        var name = $"{firstName} {lastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? (fallback ?? string.Empty) : name;
    }
}
