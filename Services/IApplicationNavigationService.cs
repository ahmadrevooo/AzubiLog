namespace AzubiLog.Services;

public interface IApplicationNavigationService
{
    IReadOnlyList<NavigationItem> GetMainNavigation();
}
