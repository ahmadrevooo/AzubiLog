using AzubiLog.Models;
using AzubiLog.Services.Identity;
using AzubiLog.Services.Timetable;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace AzubiLog.Components.Pages;

public partial class TimetablePage : ComponentBase
{
    [Inject]
    private ILogger<TimetablePage> Logger { get; set; } = null!;

    private const string EmptyDaySubtitle = "Noch keine Einträge eingetragen";
    private ApplicationUser? CurrentUser { get; set; }
    private string? StatusMessage { get; set; }
    private string? ShareStatusMessage { get; set; }
    private bool IsSaving { get; set; }
    private bool IsCopyingShareCode { get; set; }
    private bool CanManageTimetable => CurrentUser?.Role == UserRole.Klassensprecher;
    private bool HasClassAssignment => !string.IsNullOrWhiteSpace(CurrentUser?.School) && !string.IsNullOrWhiteSpace(CurrentUser?.ClassName);
    private string ShareCode => HasClassAssignment
        ? TimetableService.GenerateShareCode(CurrentUser!.School, CurrentUser.ClassName)
        : string.Empty;

    private List<DayEntry> DayEntries { get; set; } = new();
    private List<TimetableCancellation> Cancellations { get; set; } = new();

    private int SelectedCancellationDay { get; set; } = (int)DayOfWeek.Monday;
    private DateTime CancellationDate { get; set; } = DateTime.Today;
    private string? CancellationReason { get; set; }
    private string ShareCodeInput { get; set; } = string.Empty;
    private int FilledDayCount => DayEntries.Count(entry => entry.SubjectRows.Count > 0);

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await CurrentUserService.GetRequiredUserAsync();

        if (!HasClassAssignment)
            return;

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

    private async Task LoadCancellationsAsync()
    {
        if (CurrentUser is null) return;

        var allEntries = await TimetableService.GetClassTimetableAsync(
            CurrentUser.School, CurrentUser.ClassName);

        Cancellations = allEntries
            .SelectMany(e => e.Cancellations)
            .OrderByDescending(c => c.Date)
            .ToList();
    }

    private static string GetDaySubtitle(string? subjectsText)
    {
        var subjectRows = ParseSubjectRows(subjectsText);

        if (subjectRows.Count == 0)
            return EmptyDaySubtitle;

        return subjectRows.Count switch
        {
            0 => EmptyDaySubtitle,
            1 => $"{subjectRows[0].Fach} geplant",
            _ => $"{subjectRows.Count} Einträge geplant"
        };
    }

    private async Task SaveTimetableAsync()
    {
        if (CurrentUser is null || !CanManageTimetable) return;

        IsSaving = true;
        StatusMessage = null;

        try
        {
            foreach (var dayEntry in DayEntries)
            {
                await TimetableService.SaveClassTimetableAsync(
                    CurrentUser.Id,
                    CurrentUser.School,
                    CurrentUser.ClassName,
                    dayEntry.DayOfWeek,
                    SerializeSubjectRows(dayEntry.SubjectRows));
            }

            await ReloadTimetableAsync();
            StatusMessage = "Stundenplan wurde gespeichert.";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save timetable for user {UserId}", CurrentUser.Id);
            StatusMessage = "Stundenplan konnte nicht gespeichert werden.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task AddCancellationAsync()
    {
        if (CurrentUser is null || !CanManageTimetable) return;

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
        if (CurrentUser is null || !CanManageTimetable) return;

        await TimetableService.RemoveCancellationAsync(CurrentUser.Id, cancellationId);
        await LoadCancellationsAsync();
    }

    private async Task CopyShareCodeAsync()
    {
        if (!HasClassAssignment)
            return;

        IsCopyingShareCode = true;
        ShareStatusMessage = null;

        try
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", ShareCode);
            ShareStatusMessage = "Freigabecode wurde in die Zwischenablage kopiert.";
        }
        catch (JSException ex)
        {
            Logger.LogWarning(ex, "Clipboard API not available, user must copy share code manually");
            ShareStatusMessage = "Freigabecode konnte nicht kopiert werden. Bitte manuell kopieren.";
        }
        finally
        {
            IsCopyingShareCode = false;
        }
    }

    private async Task ApplyShareCodeAsync()
    {
        if (CurrentUser is null || !CanManageTimetable)
            return;

        ShareStatusMessage = null;

        var resolvedShareTarget = await TimetableService.ResolveShareCodeAsync(ShareCodeInput);
        if (resolvedShareTarget is null)
        {
            ShareStatusMessage = "Bitte gib einen gültigen Beitrittscode ein.";
            return;
        }

        var (school, className) = resolvedShareTarget.Value;

        if (string.Equals(CurrentUser.School.Trim(), school, StringComparison.OrdinalIgnoreCase)
            && string.Equals(CurrentUser.ClassName.Trim(), className, StringComparison.OrdinalIgnoreCase))
        {
            ShareStatusMessage = "Dieser Stundenplan ist bereits deiner Klasse zugeordnet.";
            return;
        }

        IsSaving = true;

        try
        {
            await TimetableService.ShareTimetableAsync(
                CurrentUser.Id,
                CurrentUser.School,
                CurrentUser.ClassName,
                school,
                className);

            ShareCodeInput = string.Empty;
            await ReloadTimetableAsync();
            ShareStatusMessage = "Stundenplan wurde für deine Klasse übernommen.";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to apply share code for user {UserId}", CurrentUser.Id);
            ShareStatusMessage = "Stundenplan konnte nicht übernommen werden.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task ReloadTimetableAsync()
    {
        if (CurrentUser is null || !HasClassAssignment)
            return;

        var entries = await TimetableService.GetClassTimetableAsync(
            CurrentUser.School, CurrentUser.ClassName);

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

        if (CanManageTimetable)
        {
            await LoadCancellationsAsync();
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

    private static List<SubjectRow> ParseSubjectRows(string? subjectsText)
    {
        if (string.IsNullOrWhiteSpace(subjectsText))
        {
            return new List<SubjectRow>();
        }

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
        catch (JsonException)
        {
            // Fall through to comma-separated parsing below
        }

        return subjectsText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(subject => new SubjectRow
            {
                Fach = subject,
                Lehrer = "Unbekannt",
                Raum = string.Empty
            })
            .ToList();
    }

    private static string SerializeSubjectRows(List<SubjectRow> subjectRows)
    {
        var normalizedRows = subjectRows
            .Where(row => !string.IsNullOrWhiteSpace(row.Fach) && !string.IsNullOrWhiteSpace(row.Lehrer))
            .Select(row => new ClassTimetableEntry.StructuredSubjectEntry
            {
                Fach = row.Fach.Trim(),
                Lehrer = row.Lehrer.Trim(),
                Raum = string.IsNullOrWhiteSpace(row.Raum) ? null : row.Raum.Trim()
            })
            .ToList();

        return normalizedRows.Count == 0
            ? string.Empty
            : JsonSerializer.Serialize(normalizedRows);
    }

    private static string FormatReadonlySubjects(string? subjectsText)
    {
        var subjectRows = ParseSubjectRows(subjectsText);
        if (subjectRows.Count == 0)
        {
            return "Keine Einträge vorhanden.";
        }

        return string.Join(Environment.NewLine, subjectRows.Select(row =>
            string.IsNullOrWhiteSpace(row.Raum)
                ? $"{row.Fach} — {row.Lehrer}"
                : $"{row.Fach} — {row.Lehrer} ({row.Raum})"));
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
