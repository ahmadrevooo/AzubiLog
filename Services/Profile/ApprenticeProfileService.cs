using System.ComponentModel.DataAnnotations;
using AzubiLog.Data;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzubiLog.Services.Profile;

public sealed class ApprenticeProfileService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager) : IApprenticeProfileService
{
    private static readonly IReadOnlyList<(DayOfWeek Day, string Label)> SchoolDays =
    [
        (DayOfWeek.Monday, "Montag"),
        (DayOfWeek.Tuesday, "Dienstag"),
        (DayOfWeek.Wednesday, "Mittwoch"),
        (DayOfWeek.Thursday, "Donnerstag"),
        (DayOfWeek.Friday, "Freitag")
    ];

    public async Task<ApprenticeProfileViewModel> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        var scheduleDays = await dbContext.SchoolScheduleDays
            .Where(scheduleDay => scheduleDay.UserId == user.Id)
            .ToListAsync(cancellationToken);

        return new ApprenticeProfileViewModel
        {
            Email = user.Email ?? string.Empty,
            Profile = new ApprenticeProfileFormModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyName = user.CompanyName,
                TrainingOccupation = user.TrainingOccupation,
                TrainingYear = user.TrainingYear <= 0 ? 1 : user.TrainingYear,
                TrainerName = user.TrainerName,
                School = user.School,
                ClassName = user.ClassName,
                Subjects = user.Subjects,
                WeeklyTargetHours = user.WeeklyTargetHours <= 0 ? 40 : user.WeeklyTargetHours,
                AnnualVacationDays = user.AnnualVacationDays <= 0 ? 30 : user.AnnualVacationDays,
                SchoolScheduleDays = BuildScheduleForm(scheduleDays)
            }
        };
    }

    public async Task SaveProfileAsync(
        ApprenticeProfileFormModel profile,
        CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(profile, new ValidationContext(profile), validateAllProperties: true);
        foreach (var scheduleDay in profile.SchoolScheduleDays)
        {
            Validator.ValidateObject(
                scheduleDay,
                new ValidationContext(scheduleDay),
                validateAllProperties: true);
        }

        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);
        user.FirstName = profile.FirstName.Trim();
        user.LastName = profile.LastName.Trim();
        user.CompanyName = profile.CompanyName.Trim();
        user.TrainingOccupation = profile.TrainingOccupation.Trim();
        user.TrainingYear = profile.TrainingYear;
        user.TrainerName = profile.TrainerName.Trim();
        user.School = profile.School.Trim();
        user.ClassName = profile.ClassName.Trim();
        user.Subjects = profile.Subjects.Trim();
        user.WeeklyTargetHours = profile.WeeklyTargetHours;
        user.AnnualVacationDays = profile.AnnualVacationDays;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(errors);
        }

        await SaveSchoolScheduleAsync(user.Id, profile.SchoolScheduleDays, cancellationToken);
    }

    private async Task SaveSchoolScheduleAsync(
        string userId,
        IReadOnlyList<SchoolScheduleDayFormModel> scheduleDays,
        CancellationToken cancellationToken)
    {
        var existingScheduleDays = await dbContext.SchoolScheduleDays
            .Where(scheduleDay => scheduleDay.UserId == userId)
            .ToListAsync(cancellationToken);

        dbContext.SchoolScheduleDays.RemoveRange(existingScheduleDays);

        var selectedScheduleDays = scheduleDays
            .Where(scheduleDay => scheduleDay.IsSelected)
            .Where(scheduleDay => SchoolDays.Any(day => day.Day == scheduleDay.DayOfWeek))
            .Select(scheduleDay => new SchoolScheduleDay
            {
                UserId = userId,
                DayOfWeek = scheduleDay.DayOfWeek,
                SubjectsText = scheduleDay.SubjectsText.Trim()
            })
            .ToList();

        dbContext.SchoolScheduleDays.AddRange(selectedScheduleDays);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<SchoolScheduleDayFormModel> BuildScheduleForm(
        IReadOnlyList<SchoolScheduleDay> scheduleDays)
    {
        return SchoolDays
            .Select(day =>
            {
                var scheduleDay = scheduleDays.FirstOrDefault(item => item.DayOfWeek == day.Day);

                return new SchoolScheduleDayFormModel
                {
                    DayOfWeek = day.Day,
                    Label = day.Label,
                    IsSelected = scheduleDay is not null,
                    SubjectsText = scheduleDay?.SubjectsText ?? string.Empty
                };
            })
            .ToList();
    }
}
