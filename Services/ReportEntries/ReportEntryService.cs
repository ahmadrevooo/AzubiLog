using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.ReportEntries;

public class ReportEntryService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IReportEntryService
{
    public async Task<ReportEntryEditorViewModel> GetEditorAsync(
        int? entryId,
        DateTime? date,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var categories = await GetCategoryOptionsAsync(user.Id, cancellationToken);
        var trainers = await GetTrainerOptionsAsync(user.Id, user.TrainerName, cancellationToken);
        var restoredDraft = false;

        ReportEntryFormModel form;
        if (entryId.HasValue)
        {
            var entry = await GetEntryForUserAsync(entryId.Value, user.Id, cancellationToken);
            form = MapToForm(entry);
        }
        else
        {
            var draft = await FindDraftAsync(user.Id, date, cancellationToken);
            restoredDraft = draft is not null;
            form = draft is null
                ? CreateNewForm(date ?? DateTime.Today, categories.FirstOrDefault()?.Id)
                : MapToForm(draft);
        }

        return await BuildEditorAsync(form, categories, trainers, restoredDraft, cancellationToken);
    }

    public async Task<ReportEntryEditorViewModel> GetFreshEditorAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var categories = await GetCategoryOptionsAsync(user.Id, cancellationToken);
        var trainers = await GetTrainerOptionsAsync(user.Id, user.TrainerName, cancellationToken);
        var form = CreateNewForm(date, categories.FirstOrDefault()?.Id);

        return await BuildEditorAsync(form, categories, trainers, false, cancellationToken);
    }

    public async Task<ReportEntryEditorViewModel> RefreshEditorAsync(
        ReportEntryFormModel form,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var categories = await GetCategoryOptionsAsync(user.Id, cancellationToken);
        var trainers = await GetTrainerOptionsAsync(user.Id, user.TrainerName, cancellationToken);

        return await BuildEditorAsync(form, categories, trainers, false, cancellationToken);
    }

    public async Task<ReportEntryFormModel> SaveDraftAsync(
        ReportEntryFormModel form,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        await ApplySmartOrderNumberAsync(form, user.Id, cancellationToken);
        var entry = await GetOrCreateEntryAsync(form, user.Id, cancellationToken);
        await ApplyFormAsync(entry, form, user.Id, ReportEntryStatus.Draft, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToForm(entry);
    }

    public async Task<int> SaveEntryAsync(
        ReportEntryFormModel form,
        CancellationToken cancellationToken = default)
    {
        ValidateForSave(form);

        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        // Check for overlapping entries
        await ValidateNoOverlapAsync(form, user.Id, cancellationToken);
        await ApplySmartOrderNumberAsync(form, user.Id, cancellationToken);
        var entry = await GetOrCreateEntryAsync(form, user.Id, cancellationToken);
        var previousWeeklyReportId = entry.WeeklyReportId;
        await ApplyFormAsync(entry, form, user.Id, ReportEntryStatus.Saved, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await UpdateWeeklyReportTotalAsync(entry.WeeklyReportId, cancellationToken);
        if (previousWeeklyReportId != default && previousWeeklyReportId != entry.WeeklyReportId)
        {
            await UpdateWeeklyReportTotalAsync(previousWeeklyReportId, cancellationToken);
        }

        return entry.Id;
    }

    public async Task DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var entry = await GetEntryForUserAsync(entryId, user.Id, cancellationToken);
        var weeklyReportId = entry.WeeklyReportId;

        dbContext.ReportEntries.Remove(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
        await UpdateWeeklyReportTotalAsync(weeklyReportId, cancellationToken);
    }

    public async Task<int> CreateCategoryAsync(string name, CancellationToken cancellationToken = default)
    {
        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ValidationException("Category name is required.");
        }

        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var existingCategory = await dbContext.Categories
            .FirstOrDefaultAsync(
                category => category.UserId == user.Id && category.Name.ToLower() == trimmedName.ToLower(),
                cancellationToken);
        if (existingCategory is not null)
        {
            return existingCategory.Id;
        }

        var nextSortOrder = await dbContext.Categories
            .Where(category => category.UserId == user.Id)
            .Select(category => (int?)category.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var category = new Category
        {
            UserId = user.Id,
            Name = trimmedName,
            ColorHex = "#2563eb",
            SortOrder = nextSortOrder + 1
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }

    public async Task<WeeklyOverviewViewModel> GetWeeklyOverviewAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        return await BuildWeeklyOverviewAsync(user.Id, date, GetWeeklyTargetHours(user), cancellationToken);
    }

    public decimal CalculateHours(string startTime, string endTime)
    {
        if (!TimeSpan.TryParse(startTime, CultureInfo.InvariantCulture, out var start)
            || !TimeSpan.TryParse(endTime, CultureInfo.InvariantCulture, out var end)
            || end <= start)
        {
            return 0m;
        }

        return Math.Round((decimal)(end - start).TotalHours, 2, MidpointRounding.AwayFromZero);
    }

    private async Task<ReportEntryEditorViewModel> BuildEditorAsync(
        ReportEntryFormModel form,
        IReadOnlyList<ReportEntryOption> categories,
        IReadOnlyList<ReportEntryOption> trainers,
        bool restoredDraft,
        CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        await ApplySmartOrderNumberAsync(form, user.Id, cancellationToken);
        var summary = await BuildDailySummaryAsync(user.Id, form.Date, cancellationToken);
        var weeklyOverview = await BuildWeeklyOverviewAsync(
            user.Id,
            form.Date,
            GetWeeklyTargetHours(user),
            cancellationToken);
        var schoolDaySuggestion = await BuildSchoolDaySuggestionAsync(
            user.Id,
            form.Date,
            categories,
            cancellationToken);

        return new ReportEntryEditorViewModel
        {
            Entry = form,
            Categories = categories,
            Trainers = trainers,
            DailySummary = summary,
            WeeklyOverview = weeklyOverview,
            SchoolDaySuggestion = schoolDaySuggestion,
            DailyReportEmailHref = BuildDailyReportEmailHref(form, trainers, summary, user),
            CalculatedHours = CalculateHours(form.StartTime, form.EndTime),
            RestoredDraft = restoredDraft
        };
    }

    private async Task<IReadOnlyList<ReportEntryOption>> GetCategoryOptionsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .Where(category => category.UserId == userId)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new ReportEntryOption(category.Id, category.Name))
            .ToListAsync(cancellationToken);

        return categories
            .Select(category => category with { Name = GetCategoryDisplayName(category.Name) })
            .ToList();
    }

    private async Task<IReadOnlyList<ReportEntryOption>> GetTrainerOptionsAsync(
        string userId,
        string profileTrainerName,
        CancellationToken cancellationToken)
    {
        await EnsureProfileTrainerAsync(userId, profileTrainerName, cancellationToken);

        return await dbContext.Trainers
            .Where(trainer => trainer.UserId == userId)
            .OrderBy(trainer => trainer.Name)
            .Select(trainer => new ReportEntryOption(trainer.Id, trainer.Name, trainer.Email, trainer.Department))
            .ToListAsync(cancellationToken);
    }

    private async Task<ReportEntry?> FindDraftAsync(
        string userId,
        DateTime? date,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ReportEntries
            .Include(entry => entry.Category)
            .Where(entry => entry.UserId == userId && entry.Status == ReportEntryStatus.Draft);

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(entry => entry.Date.Date == targetDate);
        }

        return await query
            .OrderByDescending(entry => entry.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ReportEntry> GetEntryForUserAsync(
        int entryId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ReportEntries
            .Include(entry => entry.Category)
            .FirstOrDefaultAsync(entry => entry.Id == entryId && entry.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Report entry was not found.");
    }

    private async Task<ReportEntry> GetOrCreateEntryAsync(
        ReportEntryFormModel form,
        string userId,
        CancellationToken cancellationToken)
    {
        if (form.Id.HasValue)
        {
            return await GetEntryForUserAsync(form.Id.Value, userId, cancellationToken);
        }

        var weeklyReport = await GetOrCreateWeeklyReportAsync(userId, form.Date, cancellationToken);
        var entry = new ReportEntry
        {
            UserId = userId,
            WeeklyReportId = weeklyReport.Id,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ReportEntries.Add(entry);
        return entry;
    }

    private async Task ApplyFormAsync(
        ReportEntry entry,
        ReportEntryFormModel form,
        string userId,
        string status,
        CancellationToken cancellationToken)
    {
        var weeklyReport = await GetOrCreateWeeklyReportAsync(userId, form.Date, cancellationToken);
        var duration = CalculateHours(form.StartTime, form.EndTime);

        entry.UserId = userId;
        entry.WeeklyReportId = weeklyReport.Id;
        entry.Date = form.Date.Date;
        entry.CategoryId = form.CategoryId;
        entry.TrainerId = await GetOwnedTrainerIdAsync(userId, form.TrainerId, cancellationToken);
        entry.Title = form.Title.Trim();
        entry.Description = form.Description.Trim();
        entry.Note = form.Notes.Trim();
        entry.OrderNumber = string.IsNullOrWhiteSpace(form.OrderNumber) ? null : form.OrderNumber.Trim();
        entry.Subject = string.IsNullOrWhiteSpace(form.Subject) ? null : form.Subject.Trim();
        entry.DayType = form.IsVocationalSchoolDay
            ? ReportEntryDayType.VocationalSchool
            : ReportEntryDayType.Company;
        entry.StartTime = CombineDateAndTime(form.Date, form.StartTime);
        entry.EndTime = CombineDateAndTime(form.Date, form.EndTime);
        entry.Duration = duration;
        entry.Status = status;
        entry.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<WeeklyReport> GetOrCreateWeeklyReportAsync(
        string userId,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var calendarWeek = ISOWeek.GetWeekOfYear(date);
        var year = ISOWeek.GetYear(date);

        var weeklyReport = await dbContext.WeeklyReports
            .FirstOrDefaultAsync(
                report => report.UserId == userId
                    && report.Year == year
                    && report.CalendarWeek == calendarWeek,
                cancellationToken);

        if (weeklyReport is not null)
        {
            return weeklyReport;
        }

        weeklyReport = new WeeklyReport
        {
            UserId = userId,
            Year = year,
            CalendarWeek = calendarWeek,
            Status = ReportEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WeeklyReports.Add(weeklyReport);
        await dbContext.SaveChangesAsync(cancellationToken);

        return weeklyReport;
    }

    private async Task UpdateWeeklyReportTotalAsync(int weeklyReportId, CancellationToken cancellationToken)
    {
        var weeklyReport = await dbContext.WeeklyReports
            .FirstOrDefaultAsync(report => report.Id == weeklyReportId, cancellationToken);

        if (weeklyReport is null)
        {
            return;
        }

        var total = await dbContext.ReportEntries
            .Where(entry => entry.WeeklyReportId == weeklyReportId)
            .SumAsync(entry => entry.Duration ?? 0m, cancellationToken);

        weeklyReport.TotalHours = (double)total;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<DailySummaryViewModel> BuildDailySummaryAsync(
        string userId,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var targetDate = date.Date;
        var entries = await dbContext.ReportEntries
            .Include(entry => entry.Category)
            .Where(entry => entry.UserId == userId && entry.Date.Date == targetDate)
            .OrderBy(entry => entry.StartTime)
            .Select(entry => new ReportEntryListItem(
                entry.Id,
                string.IsNullOrWhiteSpace(entry.Title) ? "(Draft)" : entry.Title,
                entry.Category == null ? "-" : GetCategoryDisplayName(entry.Category.Name),
                entry.Duration ?? 0m,
                entry.Status,
                FormatTimeRange(entry),
                entry.OrderNumber ?? string.Empty,
                CreateDescriptionPreview(entry.Description)))
            .ToListAsync(cancellationToken);

        return new DailySummaryViewModel
        {
            Date = targetDate,
            EntryCount = entries.Count,
            TotalHours = entries.Sum(entry => entry.Hours),
            Entries = entries
        };
    }

    private async Task<WeeklyOverviewViewModel> BuildWeeklyOverviewAsync(
        string userId,
        DateTime date,
        decimal weeklyTargetHours,
        CancellationToken cancellationToken)
    {
        var monday = GetMonday(date);
        var friday = monday.AddDays(4);
        var entries = await dbContext.ReportEntries
            .Include(entry => entry.Category)
            .Where(entry => entry.UserId == userId
                && entry.Date.Date >= monday
                && entry.Date.Date <= friday)
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.StartTime)
            .ToListAsync(cancellationToken);

        var days = Enumerable.Range(0, 5)
            .Select(offset =>
            {
                var day = monday.AddDays(offset);
                var dayEntries = entries
                    .Where(entry => entry.Date.Date == day)
                    .ToList();

                return new WeeklyOverviewDay
                {
                    Date = day,
                    DayKey = $"Weekday{offset + 1}",
                    EntryCount = dayEntries.Count,
                    TotalHours = dayEntries.Sum(entry => entry.Duration ?? 0m),
                    Categories = dayEntries
                        .Select(entry => entry.Category?.Name)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => GetCategoryDisplayName(name!))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Cast<string>()
                        .ToList(),
                    Entries = dayEntries
                        .Select(entry => new ReportEntryListItem(
                            entry.Id,
                            string.IsNullOrWhiteSpace(entry.Title) ? "(Draft)" : entry.Title,
                            entry.Category is null ? "-" : GetCategoryDisplayName(entry.Category.Name),
                            entry.Duration ?? 0m,
                            entry.Status,
                            FormatTimeRange(entry),
                            entry.OrderNumber ?? string.Empty,
                            CreateDescriptionPreview(entry.Description)))
                        .ToList()
                };
            })
            .ToList();

        var recordedHours = days.Sum(day => day.TotalHours);

        return new WeeklyOverviewViewModel
        {
            CalendarWeek = ISOWeek.GetWeekOfYear(date),
            WeeklyTargetHours = weeklyTargetHours,
            RecordedHours = recordedHours,
            RemainingHours = Math.Max(weeklyTargetHours - recordedHours, 0m),
            Days = days
        };
    }

    private static decimal GetWeeklyTargetHours(ApplicationUser user)
    {
        return user.WeeklyTargetHours <= 0 ? 40m : (decimal)user.WeeklyTargetHours;
    }

    private static ReportEntryFormModel CreateNewForm(DateTime date, int? defaultCategoryId)
    {
        return new ReportEntryFormModel
        {
            Date = date.Date,
            CategoryId = defaultCategoryId,
            StartTime = "08:00",
            EndTime = "16:00",
            IsDraft = true
        };
    }

    private static ReportEntryFormModel MapToForm(ReportEntry entry)
    {
        return new ReportEntryFormModel
        {
            Id = entry.Id,
            Date = entry.Date.Date,
            CategoryId = entry.CategoryId,
            Title = entry.Title,
            Description = entry.Description,
            Notes = entry.Note,
            OrderNumber = entry.OrderNumber ?? string.Empty,
            TrainerId = entry.TrainerId,
            StartTime = entry.StartTime == default ? "08:00" : entry.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            EndTime = entry.EndTime == default ? "16:00" : entry.EndTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            IsVocationalSchoolDay = entry.DayType == ReportEntryDayType.VocationalSchool,
            Subject = entry.Subject,
            IsDraft = entry.Status == ReportEntryStatus.Draft,
            IsOrderNumberOverridden = !string.IsNullOrWhiteSpace(entry.OrderNumber)
        };
    }

    private static DateTime CombineDateAndTime(DateTime date, string time)
    {
        if (!TimeSpan.TryParse(time, CultureInfo.InvariantCulture, out var parsedTime))
        {
            parsedTime = TimeSpan.Zero;
        }

        return date.Date.Add(parsedTime);
    }

    private static DateTime GetMonday(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }

    private static string GetCategoryDisplayName(string categoryName)
    {
        if (!CultureInfo.CurrentUICulture.Name.Equals("de-DE", StringComparison.OrdinalIgnoreCase))
        {
            return categoryName;
        }

        return categoryName switch
        {
            "Internal Activities" => "Interne Tätigkeiten",
            "Vocational School" => "Berufsschule",
            "Vacation" => "Urlaub",
            "Sick Leave" => "Krankheit",
            "Overtime" => "Überstunden",
            _ => categoryName
        };
    }

    private async Task ApplySmartOrderNumberAsync(
        ReportEntryFormModel form,
        string userId,
        CancellationToken cancellationToken)
    {
        if (!ShouldApplySmartOrderNumber(form))
        {
            return;
        }

        var categoryName = await GetCategoryNameAsync(userId, form.CategoryId, cancellationToken);
        form.OrderNumber = GetDefaultOrderNumber(categoryName);
    }

    private static bool ShouldApplySmartOrderNumber(ReportEntryFormModel form)
    {
        return !form.IsOrderNumberOverridden && string.IsNullOrWhiteSpace(form.OrderNumber);
    }

    private async Task<string?> GetCategoryNameAsync(
        string userId,
        int? categoryId,
        CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
        {
            return null;
        }

        return await dbContext.Categories
            .Where(category => category.UserId == userId && category.Id == categoryId.Value)
            .Select(category => category.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string GetDefaultOrderNumber(string? categoryName)
    {
        var normalizedName = NormalizeCategoryName(categoryName);

        if (normalizedName.Contains("berufsschule", StringComparison.Ordinal)
            || normalizedName.Contains("vocational school", StringComparison.Ordinal))
        {
            return "SCHULE";
        }

        if (normalizedName.Contains("urlaub", StringComparison.Ordinal)
            || normalizedName.Contains("vacation", StringComparison.Ordinal))
        {
            return "URLAUB";
        }

        if (normalizedName.Contains("krank", StringComparison.Ordinal)
            || normalizedName.Contains("sick", StringComparison.Ordinal))
        {
            return "KRANK";
        }

        if (normalizedName.Contains("überstunden", StringComparison.Ordinal)
            || normalizedName.Contains("overtime", StringComparison.Ordinal))
        {
            return "ÜBERSTUNDEN";
        }

        return "BETRIEB";
    }

    private async Task ValidateNoOverlapAsync(
        ReportEntryFormModel form,
        string userId,
        CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParseExact(form.StartTime, "hh\\:mm", System.Globalization.CultureInfo.InvariantCulture, out var newStart)
            || !TimeSpan.TryParseExact(form.EndTime, "hh\\:mm", System.Globalization.CultureInfo.InvariantCulture, out var newEnd))
        {
            return;
        }

        var entryDate = form.Date.Date;
        var dayStart = entryDate;
        var dayEnd = entryDate.AddDays(1);
        var newStartDt = entryDate.Add(newStart);
        var newEndDt = entryDate.Add(newEnd);

        var existingEntries = await dbContext.ReportEntries
            .Where(e => e.UserId == userId
                && e.Date >= dayStart && e.Date < dayEnd
                && e.Status == ReportEntryStatus.Saved
                && (!form.Id.HasValue || e.Id != form.Id.Value))
            .ToListAsync(cancellationToken);

        foreach (var existing in existingEntries)
        {
            if (existing.StartTime != default && existing.EndTime != default)
            {
                var existingStart = existing.StartTime.TimeOfDay;
                var existingEnd = existing.EndTime.TimeOfDay;

                if (newStart < existingEnd && newEnd > existingStart)
                {
                    var existStartStr = existing.StartTime.ToString("HH:mm");
                    var existEndStr = existing.EndTime.ToString("HH:mm");
                    throw new ValidationException(
                        $"Zeitüberschneidung: Es gibt bereits einen Eintrag von {existStartStr} bis {existEndStr}.");
                }
            }
        }
    }

    private async Task<SchoolDaySuggestionViewModel?> BuildSchoolDaySuggestionAsync(
        string userId,
        DateTime date,
        IReadOnlyList<ReportEntryOption> categories,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken);
        if (user is null) return null;

        // Try class timetable first (shared by Klassensprecher for the class)
        if (!string.IsNullOrWhiteSpace(user.School) && !string.IsNullOrWhiteSpace(user.ClassName))
        {
            var normalizedSchool = user.School.Trim().ToUpperInvariant();
            var normalizedClass = user.ClassName.Trim().ToUpperInvariant();

            var classTimetable = await dbContext.ClassTimetableEntries
                .Include(entry => entry.Cancellations)
                .FirstOrDefaultAsync(entry =>
                    entry.School.ToUpper() == normalizedSchool &&
                    entry.ClassName.ToUpper() == normalizedClass &&
                    entry.DayOfWeek == date.DayOfWeek,
                    cancellationToken);

            if (classTimetable is not null)
            {
                var isCancelled = classTimetable.Cancellations
                    .Any(c => c.Date.Date == date.Date);

                if (isCancelled)
                {
                    return null;
                }

                return new SchoolDaySuggestionViewModel
                {
                    IsSchoolDay = true,
                    SubjectsText = BuildSchoolDayDescription(classTimetable.SubjectsText),
                    VocationalSchoolCategoryId = FindVocationalSchoolCategoryId(categories),
                    Subjects = ParseSubjectsForDisplay(classTimetable.SubjectsText)
                };
            }
        }

        // Fall back to personal timetable (_personal + userId)
        var personalTimetable = await dbContext.ClassTimetableEntries
            .Include(entry => entry.Cancellations)
            .FirstOrDefaultAsync(entry =>
                entry.School == "_personal" &&
                entry.ClassName == userId &&
                entry.DayOfWeek == date.DayOfWeek,
                cancellationToken);

        if (personalTimetable is not null)
        {
            var isCancelled = personalTimetable.Cancellations
                .Any(c => c.Date.Date == date.Date);

            if (!isCancelled)
            {
                return new SchoolDaySuggestionViewModel
                {
                    IsSchoolDay = true,
                    SubjectsText = BuildSchoolDayDescription(personalTimetable.SubjectsText),
                    VocationalSchoolCategoryId = FindVocationalSchoolCategoryId(categories),
                    Subjects = ParseSubjectsForDisplay(personalTimetable.SubjectsText)
                };
            }
        }

        // Fall back to personal schedule (legacy)
        var scheduleDay = await dbContext.SchoolScheduleDays
            .FirstOrDefaultAsync(
                day => day.UserId == userId && day.DayOfWeek == date.DayOfWeek,
                cancellationToken);

        if (scheduleDay is null)
        {
            return null;
        }

        return new SchoolDaySuggestionViewModel
        {
            IsSchoolDay = true,
            SubjectsText = BuildSchoolDayDescription(scheduleDay.SubjectsText),
            VocationalSchoolCategoryId = FindVocationalSchoolCategoryId(categories),
            Subjects = ParseSubjectsForDisplay(scheduleDay.SubjectsText)
        };
    }

    private static int? FindVocationalSchoolCategoryId(IReadOnlyList<ReportEntryOption> categories)
    {
        return categories
            .FirstOrDefault(category => NormalizeCategoryName(category.Name).Contains("berufsschule", StringComparison.Ordinal))
            ?.Id;
    }

    private static string BuildSchoolDayDescription(string subjectsText)
    {
        var structuredSubjects = TryParseStructuredSubjects(subjectsText);
        if (structuredSubjects.Count > 0)
        {
            return "Berufsschultag: " + string.Join(", ", structuredSubjects.Select(FormatStructuredSubject));
        }

        var subjects = subjectsText
            .Split(["\r\n", "\n", ","], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (subjects.Count == 0)
        {
            return "Berufsschultag";
        }

        return "Berufsschultag\n\n"
            + string.Join("\n", subjects.Select(subject => $"• {subject}"));
    }

    private static List<ClassTimetableEntry.StructuredSubjectEntry> TryParseStructuredSubjects(string subjectsText)
    {
        if (string.IsNullOrWhiteSpace(subjectsText))
        {
            return [];
        }

        var trimmedSubjectsText = subjectsText.Trim();

        // Try new DayDataJson format (object with Status and Entries)
        if (trimmedSubjectsText.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                var dayData = JsonSerializer.Deserialize<DayDataJsonDto>(
                    trimmedSubjectsText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dayData?.Entries is not null)
                {
                    return dayData.Entries
                        .Where(e => !string.IsNullOrWhiteSpace(e.Fach))
                        .Select(e => new ClassTimetableEntry.StructuredSubjectEntry
                        {
                            Fach = e.Fach,
                            Lehrer = e.Lehrer ?? string.Empty,
                            Raum = e.Raum
                        })
                        .ToList();
                }
            }
            catch (JsonException) { }
        }

        // Try legacy array format
        if (!trimmedSubjectsText.StartsWith("[", StringComparison.Ordinal))
        {
            return [];
        }

        try
        {
            var structuredSubjects = JsonSerializer.Deserialize<List<ClassTimetableEntry.StructuredSubjectEntry>>(
                trimmedSubjectsText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return structuredSubjects?
                .Where(subject => !string.IsNullOrWhiteSpace(subject.Fach))
                .ToList()
                ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<SchoolSubjectEntry> ParseSubjectsForDisplay(string subjectsText)
    {
        var trimmed = subjectsText?.Trim() ?? string.Empty;

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            try
            {
                var dayData = JsonSerializer.Deserialize<DayDataJsonDto>(
                    trimmed,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dayData?.Entries is not null)
                {
                    return dayData.Entries
                        .Where(e => !string.IsNullOrWhiteSpace(e.Fach))
                        .Select(e => new SchoolSubjectEntry
                        {
                            Fach = e.Fach.Trim(),
                            Lehrer = e.Lehrer?.Trim() ?? string.Empty,
                            Raum = e.Raum?.Trim(),
                            Entfall = e.Entfall
                        })
                        .ToList();
                }
            }
            catch (JsonException) { }
        }

        var structured = TryParseStructuredSubjects(subjectsText ?? string.Empty);
        return structured
            .Select(s => new SchoolSubjectEntry
            {
                Fach = s.Fach.Trim(),
                Lehrer = s.Lehrer?.Trim() ?? string.Empty,
                Raum = s.Raum?.Trim()
            })
            .ToList();
    }

    private sealed class DayDataJsonDto
    {
        public string? Status { get; set; }
        public List<DayDataEntryDto>? Entries { get; set; }
    }

    private sealed class DayDataEntryDto
    {
        public string Fach { get; set; } = string.Empty;
        public string? Lehrer { get; set; }
        public string? Raum { get; set; }
        public bool Entfall { get; set; }
    }

    private static string FormatStructuredSubject(ClassTimetableEntry.StructuredSubjectEntry subject)
    {
        var details = new List<string>();

        if (!string.IsNullOrWhiteSpace(subject.Lehrer))
        {
            details.Add(subject.Lehrer.Trim());
        }

        if (!string.IsNullOrWhiteSpace(subject.Raum))
        {
            details.Add(subject.Raum.Trim());
        }

        var subjectName = subject.Fach.Trim();
        return details.Count > 0
            ? $"{subjectName} ({string.Join(", ", details)})"
            : subjectName;
    }

    private static string NormalizeCategoryName(string? categoryName)
    {
        return (categoryName ?? string.Empty).Trim().ToLowerInvariant();
    }

    private async Task EnsureProfileTrainerAsync(
        string userId,
        string profileTrainerName,
        CancellationToken cancellationToken)
    {
        var trimmedName = profileTrainerName.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return;
        }

        var exists = await dbContext.Trainers
            .AnyAsync(
                trainer => trainer.UserId == userId && trainer.Name.ToLower() == trimmedName.ToLower(),
                cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.Trainers.Add(new Trainer
        {
            UserId = userId,
            Name = trimmedName
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int?> GetOwnedTrainerIdAsync(
        string userId,
        int? trainerId,
        CancellationToken cancellationToken)
    {
        if (!trainerId.HasValue)
        {
            return null;
        }

        return await dbContext.Trainers
            .Where(trainer => trainer.UserId == userId && trainer.Id == trainerId.Value)
            .Select(trainer => (int?)trainer.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string FormatTimeRange(ReportEntry entry)
    {
        if (entry.StartTime == default || entry.EndTime == default)
        {
            return string.Empty;
        }

        return $"{entry.StartTime:HH:mm} - {entry.EndTime:HH:mm}";
    }

    private static string CreateDescriptionPreview(string description)
    {
        var normalizedDescription = description.Trim();
        if (normalizedDescription.Length <= 180)
        {
            return normalizedDescription;
        }

        return normalizedDescription[..177] + "...";
    }

    private static string BuildDailyReportEmailHref(
    ReportEntryFormModel form,
    IReadOnlyList<ReportEntryOption> trainers,
    DailySummaryViewModel summary,
    ApplicationUser user)
    {
        var trainer = trainers.FirstOrDefault(trainer => trainer.Id == form.TrainerId);
        var trainerEmail = trainer?.Email ?? string.Empty;
        var trainerName = trainer?.Name ?? string.Empty;

        var userName = $"{user.FirstName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = user.Email ?? "Unbekannt";
        }

        var subject = $"Tagesbericht vom {form.Date:dd.MM.yyyy}";
        const string lineBreak = "\r\n";
        const string separator = "--------------------------------";

        var lines = new List<string>
    {
        $"Tagesbericht von {userName}",
        separator,
        string.Empty,
        $"Datum: {summary.Date:dddd, dd.MM.yyyy}",
        $"Gesamte Arbeitszeit: {summary.TotalHours:0.##} h"
    };

        if (!string.IsNullOrWhiteSpace(trainerName))
        {
            lines.Add($"Ausbilder: {trainerName}");
        }

        lines.AddRange(
        [
            string.Empty,
            "Einträge",
            separator,
            string.Empty
        ]);

        if (summary.Entries.Count == 0)
        {
            lines.Add("Keine Einträge vorhanden.");
        }
        else
        {
            foreach (var entry in summary.Entries)
            {
                lines.Add(entry.TimeRange);
                lines.Add($"Dauer: {entry.Hours:0.##} h");

                lines.Add($"Tätigkeit: {entry.Title}");
                lines.Add($"Beschreibung: {entry.DescriptionPreview}");
                lines.Add(string.Empty);
            }
        }

        lines.Add(separator);
        lines.Add("Erstellt mit AzubiLog");

        var body = string.Join(lineBreak, lines);

        return $"mailto:{Uri.EscapeDataString(trainerEmail)}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
    }

    private static void ValidateForSave(ReportEntryFormModel form)
    {
        var context = new ValidationContext(form);
        Validator.ValidateObject(form, context, validateAllProperties: true);

        if (form.Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            throw new ValidationException("Einträge können nur für Werktage (Montag bis Freitag) erstellt werden.");
        }
    }
}
