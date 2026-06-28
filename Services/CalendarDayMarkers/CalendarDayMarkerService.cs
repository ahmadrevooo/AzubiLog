using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.CalendarDayMarkers;

public class CalendarDayMarkerService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : ICalendarDayMarkerService
{
    public async Task<IReadOnlyList<CalendarDayMarker>> GetMarkersForMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var firstDay = new DateOnly(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        return await dbContext.CalendarDayMarkers
            .AsNoTracking()
            .Where(marker => marker.UserId == user.Id
                && marker.Date >= firstDay
                && marker.Date <= lastDay)
            .OrderBy(marker => marker.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<CalendarDayMarker?> GetMarkerAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        return await dbContext.CalendarDayMarkers
            .AsNoTracking()
            .FirstOrDefaultAsync(marker => marker.UserId == user.Id && marker.Date == date, cancellationToken);
    }

    public async Task SetMarkerAsync(
        DateOnly date,
        CalendarDayType type,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var marker = await dbContext.CalendarDayMarkers
            .FirstOrDefaultAsync(existing => existing.UserId == user.Id && existing.Date == date, cancellationToken);
        var now = DateTime.UtcNow;

        if (marker is null)
        {
            dbContext.CalendarDayMarkers.Add(new CalendarDayMarker
            {
                UserId = user.Id,
                Date = date,
                Type = type,
                CreatedAt = now
            });
        }
        else
        {
            marker.Type = type;
            marker.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMarkerAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var marker = await dbContext.CalendarDayMarkers
            .FirstOrDefaultAsync(existing => existing.UserId == user.Id && existing.Date == date, cancellationToken);

        if (marker is null)
        {
            return;
        }

        dbContext.CalendarDayMarkers.Remove(marker);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
