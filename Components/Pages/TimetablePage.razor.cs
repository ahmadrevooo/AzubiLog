using AzubiLog.Models;
using AzubiLog.Services.Identity;
using AzubiLog.Services.Timetable;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AzubiLog.Components.Pages;

public partial class TimetablePage : ComponentBase
{
    private const string EmptyDaySubtitle = "Noch keine Fächer eingetragen";
    private ApplicationUser? CurrentUser { get; set; }
    private string? StatusMessage { get; set; }
    private string? ShareStatusMessage { get; set; }
    private bool IsSaving { get; set; }
    private bool IsCopyingShareCode { get; set; }
    private bool CanManageTimetable => CurrentUser?.Role == UserRole.Klassensprecher;
    private bool HasClassAssignment => !string.IsNullOrWhiteSpace(CurrentUser?.School) && !string.IsNullOrWhiteSpace(CurrentUser?.ClassName);
    private string ShareCode => HasClassAssignment
        ? $"{CurrentUser!.School.Trim()}|{CurrentUser.ClassName.Trim()}"
        : string.Empty;

    private List<DayEntry> DayEntries { get; set; } = new();
    private List<TimetableCancellation> Cancellations { get; set; } = new();

    private int SelectedCancellationDay { get; set; } = (int)DayOfWeek.Monday;
    private DateTime CancellationDate { get; set; } = DateTime.Today;
    private string? CancellationReason { get; set; }
    private string ShareCodeInput { get; set; } = string.Empty;
    private int FilledDayCount => DayEntries.Count(entry => !string.IsNullOrWhiteSpace(entry.SubjectsText));

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

    private void OnSubjectsChanged(DayOfWeek day, string? value)
    {
        var entry = DayEntries.FirstOrDefault(d => d.DayOfWeek == day);
        if (entry is not null)
        {
            entry.SubjectsText = value ?? string.Empty;
        }
    }

    private static string GetDaySubtitle(string? subjectsText)
    {
        if (string.IsNullOrWhiteSpace(subjectsText))
            return EmptyDaySubtitle;

        var subjects = subjectsText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return subjects.Length switch
        {
            0 => EmptyDaySubtitle,
            1 => $"{subjects[0]} geplant",
            _ => $"{subjects.Length} Fächer geplant"
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
                    dayEntry.SubjectsText);
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
        catch
        {
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

        if (!TryParseShareCode(ShareCodeInput, out var school, out var className))
        {
            ShareStatusMessage = "Bitte gib einen gültigen Freigabecode im Format Schule|Klasse ein.";
            return;
        }

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
        catch
        {
            ShareStatusMessage = "Stundenplan konnte nicht geteilt werden.";
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
        }

        foreach (var entry in entries)
        {
            var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == entry.DayOfWeek);
            if (dayEntry is not null)
            {
                dayEntry.SubjectsText = entry.SubjectsText;
                dayEntry.EntryId = entry.Id;
            }
        }

        if (CanManageTimetable)
        {
            await LoadCancellationsAsync();
        }
    }

    private static bool TryParseShareCode(string? shareCode, out string school, out string className)
    {
        school = string.Empty;
        className = string.Empty;

        if (string.IsNullOrWhiteSpace(shareCode))
            return false;

        var parts = shareCode
            .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            return false;

        school = parts[0];
        className = parts[1];
        return !string.IsNullOrWhiteSpace(school) && !string.IsNullOrWhiteSpace(className);
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
    }
}
