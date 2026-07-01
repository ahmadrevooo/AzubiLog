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
        var trainers = await dbContext.Trainers
            .Where(trainer => trainer.UserId == user.Id)
            .OrderBy(trainer => trainer.Name)
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
                PdfAccentColor = string.IsNullOrWhiteSpace(user.PdfAccentColor) ? "#2563eb" : user.PdfAccentColor,
                WeeklyTargetHours = user.WeeklyTargetHours <= 0 ? 40 : user.WeeklyTargetHours,
                AnnualVacationDays = user.AnnualVacationDays <= 0 ? 30 : user.AnnualVacationDays,
                SchoolScheduleDays = BuildScheduleForm(scheduleDays),
                Trainers = BuildTrainerForm(user.TrainerName, trainers)
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
        foreach (var trainer in profile.Trainers)
        {
            Validator.ValidateObject(
                trainer,
                new ValidationContext(trainer),
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
        user.PdfAccentColor = string.IsNullOrWhiteSpace(profile.PdfAccentColor) ? "#2563eb" : profile.PdfAccentColor.Trim();
        user.WeeklyTargetHours = profile.WeeklyTargetHours;
        user.AnnualVacationDays = profile.AnnualVacationDays;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(errors);
        }

        await SaveSchoolScheduleAsync(user.Id, profile.SchoolScheduleDays, cancellationToken);
        await SaveTrainersAsync(user.Id, profile.TrainerName, profile.Trainers, cancellationToken);
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

    private async Task SaveTrainersAsync(
        string userId,
        string profileTrainerName,
        IReadOnlyList<TrainerFormModel> trainers,
        CancellationToken cancellationToken)
    {
        var existingTrainers = await dbContext.Trainers
            .Where(trainer => trainer.UserId == userId)
            .ToListAsync(cancellationToken);
        var submittedTrainers = trainers
            .Where(trainer => !string.IsNullOrWhiteSpace(trainer.Name))
            .ToList();
        var submittedIds = submittedTrainers
            .Where(trainer => trainer.Id.HasValue)
            .Select(trainer => trainer.Id!.Value)
            .ToHashSet();

        dbContext.Trainers.RemoveRange(existingTrainers.Where(trainer => !submittedIds.Contains(trainer.Id)));

        foreach (var submittedTrainer in submittedTrainers)
        {
            var trainer = submittedTrainer.Id.HasValue
                ? existingTrainers.FirstOrDefault(item => item.Id == submittedTrainer.Id.Value)
                : null;

            if (trainer is null)
            {
                trainer = new Trainer { UserId = userId };
                dbContext.Trainers.Add(trainer);
            }

            trainer.Name = submittedTrainer.Name.Trim();
            trainer.Email = submittedTrainer.Email.Trim();
            trainer.Department = submittedTrainer.Department.Trim();
        }

        if (!string.IsNullOrWhiteSpace(profileTrainerName)
            && !submittedTrainers.Any(trainer => trainer.Name.Trim().Equals(profileTrainerName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            dbContext.Trainers.Add(new Trainer
            {
                UserId = userId,
                Name = profileTrainerName.Trim()
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<TrainerFormModel> BuildTrainerForm(
        string profileTrainerName,
        IReadOnlyList<Trainer> trainers)
    {
        var trainerForms = trainers
            .Select(trainer => new TrainerFormModel
            {
                Id = trainer.Id,
                Name = trainer.Name,
                Email = trainer.Email,
                Department = trainer.Department
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(profileTrainerName)
            && !trainerForms.Any(trainer => trainer.Name.Equals(profileTrainerName, StringComparison.OrdinalIgnoreCase)))
        {
            trainerForms.Insert(0, new TrainerFormModel { Name = profileTrainerName });
        }

        return trainerForms;
    }
}
