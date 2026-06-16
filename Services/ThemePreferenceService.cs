namespace AzubiLog.Services;

public class ThemePreferenceService : IThemePreferenceService
{
    public ThemeMode DefaultTheme => ThemeMode.Light;

    public IReadOnlyList<ThemeMode> SupportedThemes { get; } =
    [
        ThemeMode.Light,
        ThemeMode.Dark
    ];
}
