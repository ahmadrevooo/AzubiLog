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
    private List<TimetableCancellation> Cancellations { get; set; } = new();

    private int SelectedCancellationDay { get; set; } = (int)DayOfWeek.Monday;
    private DateTime CancellationDate { get; set; } = DateTime.Today;
    private string? CancellationReason { get; set; }
    private int FilledDayCount => DayEntries.Count(entry => entry.SubjectRows.Count > 0);

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

    private async Task SaveTimetableAsync()
    {
        if (CurrentUser is null) return;

        IsSaving = true;
        StatusMessage = null;

        try
        {
            foreach (var dayEntry in DayEntries)
            {
                await TimetableService.SaveClassTimetableAsync(
                    CurrentUser.Id,
                    UserSchool,
                    UserClass,
                    dayEntry.DayOfWeek,
                    SerializeSubjectRows(dayEntry.SubjectRows));
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

    private async Task AddCancellationAsync()
    {
        if (CurrentUser is null) return;

        var selectedDay = (DayOfWeek)SelectedCancellationDay;
        var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == selectedDay);
        if (dayEntry is null || dayEntry.EntryId is null) return;

        IsSaving = true;
        try
        {
            await TimetableService.AddCancellationAsync(
                CurrentUser.Id,
                dayEntry.EntryId.Value,
                CancellationDate,
                CancellationReason);

            CancellationReason = null;
            await LoadCancellationsAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task RemoveCancellationAsync(int cancellationId)
    {
        if (CurrentUser is null) return;

        await TimetableService.RemoveCancellationAsync(CurrentUser.Id, cancellationId);
        await LoadCancellationsAsync();
    }

    private async Task LoadCancellationsAsync()
    {
        if (CurrentUser is null) return;

        var allEntries = await TimetableService.GetClassTimetableAsync(UserSchool, UserClass);

        Cancellations = allEntries
            .SelectMany(e => e.Cancellations)
            .OrderByDescending(c => c.Date)
            .ToList();
    }

    private async Task ReloadTimetableAsync()
    {
        if (CurrentUser is null) return;

        var entries = await TimetableService.GetClassTimetableAsync(UserSchool, UserClass);

        foreach (var dayEntry in DayEntries)
        {
            dayEntry.SubjectsText = string.Empty;
            dayEntry.EntryId = null;
            dayEntry.SubjectRows.Clear();
        }

        foreach (var entry in entries)
        {
            var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == entry.DayOfWeek);
            if (dayEntry is not null)
            {
                dayEntry.SubjectsText = entry.SubjectsText;
                dayEntry.EntryId = entry.Id;
                dayEntry.SubjectRows = ParseSubjectRows(entry.SubjectsText);
            }
        }

        await LoadCancellationsAsync();
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

    private static List<SubjectRow> ParseSubjectRows(string? subjectsText)
    {
        if (string.IsNullOrWhiteSpace(subjectsText))
            return new List<SubjectRow>();

        try
        {
            var structuredEntries = JsonSerializer.Deserialize<List<ClassTimetableEntry.StructuredSubjectEntry>>(subjectsText);
            if (structuredEntries is not null)
            {
                return structuredEntries
                    .Where(entry => !string.IsNullOrWhiteSpace(entry.Fach) && !string.IsNullOrWhiteSpace(entry.Lehrer))
                    .Select(entry => new SubjectRow
                    {
                        Fach = entry.Fach.Trim(),
                        Lehrer = entry.Lehrer.Trim(),
                        Raum = entry.Raum?.Trim() ?? string.Empty
                    })
                    .ToList();
            }
        }
        catch (JsonException) { }

        return subjectsText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(subject => new SubjectRow
            {
                Fach = subject,
                Lehrer = string.Empty,
                Raum = string.Empty
            })
            .ToList();
    }

    private static string SerializeSubjectRows(List<SubjectRow> subjectRows)
    {
        var normalizedRows = subjectRows
            .Where(row => !string.IsNullOrWhiteSpace(row.Fach))
            .Select(row => new ClassTimetableEntry.StructuredSubjectEntry
            {
                Fach = row.Fach.Trim(),
                Lehrer = string.IsNullOrWhiteSpace(row.Lehrer) ? "-" : row.Lehrer.Trim(),
                Raum = string.IsNullOrWhiteSpace(row.Raum) ? null : row.Raum.Trim()
            })
            .ToList();

        return normalizedRows.Count == 0
            ? string.Empty
            : JsonSerializer.Serialize(normalizedRows);
    }

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
        public string SubjectsText { get; set; } = string.Empty;
        public int? EntryId { get; set; }
        public List<SubjectRow> SubjectRows { get; set; } = new();
    }

    private sealed class SubjectRow
    {
        public string Fach { get; set; } = string.Empty;
        public string Lehrer { get; set; } = string.Empty;
        public string Raum { get; set; } = string.Empty;
    }
}
