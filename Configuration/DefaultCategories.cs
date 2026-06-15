namespace AzubiLog.Configuration;

/// <summary>Defines the default categories created for every new apprentice.</summary>
public static class DefaultCategories
{
    public static readonly IReadOnlyList<(string Name, string ColorHex)> All =
    [
        ("Internal Activities", "#3768E5"),
        ("Vocational School", "#795AE5"),
        ("Vacation", "#E39C2E"),
        ("Sick Leave", "#EF5E65"),
        ("Overtime", "#35A56C")
    ];
}
