using AzubiLog.Models;
using AzubiLog.Services.Timetable;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace AzubiLog.Components.Pages;

public partial class TimetablePage : ComponentBase
{
    private ApplicationUser? CurrentUser { get; set; }
    private string? StatusMessage { get; set; }
    private bool IsSaving { get; set; }
    private DayOfWeek SelectedDay { get; set; } = DayOfWeek.Monday;

    private List<DayEntry> DayEntries { get; set; } = new();
    private int FilledDayCount => DayEntries.Count(entry => entry.SubjectRows.Count > 0);

    private static readonly KeyValuePair<string, string>[] StatusOptions =
    [
        new("normal", "Normal"),
        new("krank", "Krank"),
        new("urlaub", "Urlaub"),
        new("betrieb", "Betrieb"),
        new("feiertag", "Feiertag")
    ];

    private string UserSchool => string.IsNullOrWhiteSpace(CurrentUser?.School) ? "_personal" : CurrentUser.School;
    private string UserClass => string.IsNullOrWhiteSpace(CurrentUser?.ClassName) ? CurrentUser!.Id : CurrentUser.ClassName;

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await CurrentUserService.GetRequiredUserAsync();

        DayEntries = new List<DayEntry>
        {
            new(DayOfWeek.Monday),
            new(DayOfWeek.Tuesday),
            new(DayOfWeek.Wednesday),
            new(DayOfWeek.Thursday),
            new(DayOfWeek.Friday)
        };

        await ReloadTimetableAsync();
    }

    private void SelectDay(DayOfWeek day)
    {
        SelectedDay = day;
    }

    private void SetDayStatus(DayEntry dayEntry, string status)
    {
        dayEntry.DayStatus = status;
        if (status != "normal")
        {
            foreach (var row in dayEntry.SubjectRows)
                row.Entfall = false;
        }
    }

    private void ToggleEntfall(SubjectRow row)
    {
        row.Entfall = !row.Entfall;
    }

    private async Task SaveTimetableAsync()
    {
        if (CurrentUser is null) return;

        IsSaving = true;
        StatusMessage = null;

        try
        {
            foreach (var dayEntry in DayEntries)
            {
                var serialized = SerializeDayData(dayEntry);
                await TimetableService.SaveClassTimetableAsync(
                    CurrentUser.Id,
                    UserSchool,
                    UserClass,
                    dayEntry.DayOfWeek,
                    serialized);
            }

            await ReloadTimetableAsync();
            StatusMessage = "Stundenplan wurde gespeichert.";
        }
        catch
        {
            StatusMessage = "Stundenplan konnte nicht gespeichert werden.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task ReloadTimetableAsync()
    {
        if (CurrentUser is null) return;

        var entries = await TimetableService.GetClassTimetableAsync(UserSchool, UserClass);

        foreach (var dayEntry in DayEntries)
        {
            dayEntry.SubjectRows.Clear();
            dayEntry.DayStatus = "normal";
        }

        foreach (var entry in entries)
        {
            var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == entry.DayOfWeek);
            if (dayEntry is not null)
            {
                ParseDayData(entry.SubjectsText, dayEntry);
            }
        }
    }

    private void AddSubjectRow(DayOfWeek dayOfWeek)
    {
        var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);
        dayEntry?.SubjectRows.Add(new SubjectRow());
    }

    private void RemoveSubjectRow(DayOfWeek dayOfWeek, SubjectRow row)
    {
        var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);
        dayEntry?.SubjectRows.Remove(row);
    }

    private static void ParseDayData(string? subjectsText, DayEntry dayEntry)
    {
        if (string.IsNullOrWhiteSpace(subjectsText))
            return;

        try
        {
            var dayData = JsonSerializer.Deserialize<DayDataJson>(subjectsText);
            if (dayData?.Entries is not null)
            {
                dayEntry.DayStatus = dayData.Status ?? "normal";
                dayEntry.SubjectRows = dayData.Entries
                    .Where(e => !string.IsNullOrWhiteSpace(e.Fach))
                    .Select(e => new SubjectRow
                    {
                        Fach = e.Fach.Trim(),
                        Lehrer = e.Lehrer?.Trim() ?? string.Empty,
                        Raum = e.Raum?.Trim() ?? string.Empty,
                        Entfall = e.Entfall
                    })
                    .ToList();
                return;
            }
        }
        catch (JsonException) { }

        try
        {
            var legacyEntries = JsonSerializer.Deserialize<List<ClassTimetableEntry.StructuredSubjectEntry>>(subjectsText);
            if (legacyEntries is not null)
            {
                dayEntry.SubjectRows = legacyEntries
                    .Where(e => !string.IsNullOrWhiteSpace(e.Fach))
                    .Select(e => new SubjectRow
                    {
                        Fach = e.Fach.Trim(),
                        Lehrer = e.Lehrer?.Trim() ?? string.Empty,
                        Raum = e.Raum?.Trim() ?? string.Empty,
                        Entfall = false
                    })
                    .ToList();
                return;
            }
        }
        catch (JsonException) { }

        dayEntry.SubjectRows = subjectsText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => new SubjectRow { Fach = s, Lehrer = string.Empty, Raum = string.Empty })
            .ToList();
    }

    private static string SerializeDayData(DayEntry dayEntry)
    {
        var entries = dayEntry.SubjectRows
            .Where(row => !string.IsNullOrWhiteSpace(row.Fach))
            .Select(row => new SubjectEntryJson
            {
                Fach = row.Fach.Trim(),
                Lehrer = string.IsNullOrWhiteSpace(row.Lehrer) ? "-" : row.Lehrer.Trim(),
                Raum = string.IsNullOrWhiteSpace(row.Raum) ? null : row.Raum.Trim(),
                Entfall = row.Entfall
            })
            .ToList();

        if (entries.Count == 0 && dayEntry.DayStatus == "normal")
            return string.Empty;

        var dayData = new DayDataJson
        {
            Status = dayEntry.DayStatus,
            Entries = entries
        };

        return JsonSerializer.Serialize(dayData);
    }

    private static string GetStatusLabel(string status) => status switch
    {
        "krank" => "Krank",
        "urlaub" => "Urlaub",
        "betrieb" => "Betrieb",
        "feiertag" => "Feiertag",
        _ => "Normal"
    };

    private static string GetDayName(DayOfWeek day) => day switch
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

    private static string GetDayShort(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Mo",
        DayOfWeek.Tuesday => "Di",
        DayOfWeek.Wednesday => "Mi",
        DayOfWeek.Thursday => "Do",
        DayOfWeek.Friday => "Fr",
        _ => day.ToString()[..2]
    };

    private sealed class DayEntry(DayOfWeek dayOfWeek)
    {
        public DayOfWeek DayOfWeek { get; } = dayOfWeek;
        public string DayStatus { get; set; } = "normal";
        public List<SubjectRow> SubjectRows { get; set; } = new();
    }

    private sealed class SubjectRow
    {
        public string Fach { get; set; } = string.Empty;
        public string Lehrer { get; set; } = string.Empty;
        public string Raum { get; set; } = string.Empty;
        public bool Entfall { get; set; }
    }

    private sealed class DayDataJson
    {
        public string? Status { get; set; }
        public List<SubjectEntryJson> Entries { get; set; } = new();
    }

    private sealed class SubjectEntryJson
    {
        public string Fach { get; set; } = string.Empty;
        public string Lehrer { get; set; } = string.Empty;
        public string? Raum { get; set; }
        public bool Entfall { get; set; }
    }
}
