using AzubiLog.Models;

namespace AzubiLog.Services.CalendarDayMarkers;

public interface ICalendarDayMarkerService
{
    Task<IReadOnlyList<CalendarDayMarker>> GetMarkersForMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default);

    Task<CalendarDayMarker?> GetMarkerAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task SetMarkerAsync(
        DateOnly date,
        CalendarDayType type,
        CancellationToken cancellationToken = default);

    Task RemoveMarkerAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);
}
