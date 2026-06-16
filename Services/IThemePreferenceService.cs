namespace AzubiLog.Services;

public interface IThemePreferenceService
{
    ThemeMode DefaultTheme { get; }
    IReadOnlyList<ThemeMode> SupportedThemes { get; }
}
