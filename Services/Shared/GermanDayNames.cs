namespace AzubiLog.Services.Shared;

public static class GermanDayNames
{
    private static readonly IReadOnlyList<(DayOfWeek Day, string Label)> Weekdays =
    [
        (DayOfWeek.Monday, "Montag"),
        (DayOfWeek.Tuesday, "Dienstag"),
        (DayOfWeek.Wednesday, "Mittwoch"),
        (DayOfWeek.Thursday, "Donnerstag"),
        (DayOfWeek.Friday, "Freitag")
    ];

    public static IReadOnlyList<(DayOfWeek Day, string Label)> WorkWeek => Weekdays;

    public static string GetName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Montag",
        DayOfWeek.Tuesday => "Dienstag",
        DayOfWeek.Wednesday => "Mittwoch",
        DayOfWeek.Thursday => "Donnerstag",
        DayOfWeek.Friday => "Freitag",
        DayOfWeek.Saturday => "Samstag",
        DayOfWeek.Sunday => "Sonntag",
        _ => day.ToString()
    };

    public static string GetNameByOffset(int offset) => offset switch
    {
        0 => "Montag",
        1 => "Dienstag",
        2 => "Mittwoch",
        3 => "Donnerstag",
        _ => "Freitag"
    };
}
