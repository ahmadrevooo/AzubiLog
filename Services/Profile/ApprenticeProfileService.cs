using System.ComponentModel.DataAnnotations;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.AspNetCore.Identity;

namespace AzubiLog.Services.Profile;

public sealed class ApprenticeProfileService(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager) : IApprenticeProfileService
{
    public async Task<ApprenticeProfileViewModel> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

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
                AnnualVacationDays = user.AnnualVacationDays <= 0 ? 30 : user.AnnualVacationDays
            }
        };
    }

    public async Task SaveProfileAsync(
        ApprenticeProfileFormModel profile,
        CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(profile, new ValidationContext(profile), validateAllProperties: true);

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
    }
}
