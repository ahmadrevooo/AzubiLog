namespace AzubiLog.Services.Shared;

public static class DateHelpers
{
    public static DateTime GetMonday(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }
}
