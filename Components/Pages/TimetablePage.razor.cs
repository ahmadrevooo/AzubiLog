using AzubiLog.Models;
using AzubiLog.Services.Identity;
using AzubiLog.Services.Timetable;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class TimetablePage : ComponentBase
{
    private ApplicationUser? CurrentUser { get; set; }
    private string? StatusMessage { get; set; }
    private bool IsSaving { get; set; }

    private List<DayEntry> DayEntries { get; set; } = new();
    private List<TimetableCancellation> Cancellations { get; set; } = new();

    private int SelectedCancellationDay { get; set; } = (int)DayOfWeek.Monday;
    private DateTime CancellationDate { get; set; } = DateTime.Today;
    private string? CancellationReason { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await CurrentUserService.GetRequiredUserAsync();

        if (CurrentUser.Role != UserRole.Klassensprecher)
            return;

        DayEntries = new List<DayEntry>
        {
            new(DayOfWeek.Monday),
            new(DayOfWeek.Tuesday),
            new(DayOfWeek.Wednesday),
            new(DayOfWeek.Thursday),
            new(DayOfWeek.Friday)
        };

        var entries = await TimetableService.GetClassTimetableAsync(
            CurrentUser.School, CurrentUser.ClassName);

        foreach (var entry in entries)
        {
            var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == entry.DayOfWeek);
            if (dayEntry is not null)
            {
                dayEntry.SubjectsText = entry.SubjectsText;
                dayEntry.EntryId = entry.Id;
            }
        }

        await LoadCancellationsAsync();
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
                    CurrentUser.School,
                    CurrentUser.ClassName,
                    dayEntry.DayOfWeek,
                    dayEntry.SubjectsText);
            }

            var entries = await TimetableService.GetClassTimetableAsync(
                CurrentUser.School, CurrentUser.ClassName);

            foreach (var entry in entries)
            {
                var dayEntry = DayEntries.FirstOrDefault(d => d.DayOfWeek == entry.DayOfWeek);
                if (dayEntry is not null)
                {
                    dayEntry.EntryId = entry.Id;
                }
            }

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
