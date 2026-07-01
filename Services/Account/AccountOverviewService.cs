using System.ComponentModel.DataAnnotations;
using AzubiLog.Models;
using AzubiLog.Services.Identity;
using Microsoft.AspNetCore.Identity;

namespace AzubiLog.Services.Account;

public sealed class AccountOverviewService(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager) : IAccountOverviewService
{
    public async Task<AccountOverviewViewModel> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

        return new AccountOverviewViewModel
        {
            Email = user.Email ?? string.Empty,
            FullName = string.IsNullOrWhiteSpace(fullName) ? user.Email ?? string.Empty : fullName,
            TrainingOccupation = user.TrainingOccupation,
            CompanyName = user.CompanyName,
            TrainingYear = user.TrainingYear <= 0 ? 1 : user.TrainingYear,
            Account = new AccountOverviewFormModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                WeeklyTargetHours = user.WeeklyTargetHours <= 0 ? 40 : user.WeeklyTargetHours,
                AnnualVacationDays = user.AnnualVacationDays <= 0 ? 30 : user.AnnualVacationDays,
                CompanyName = user.CompanyName,
                TrainingOccupation = user.TrainingOccupation,
                TrainingYear = user.TrainingYear <= 0 ? 1 : user.TrainingYear
            }
        };
    }

    public async Task SaveAccountAsync(
        AccountOverviewFormModel account,
        CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(account, new ValidationContext(account), validateAllProperties: true);

        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        var newEmail = account.Email.Trim();
        if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await userManager.FindByEmailAsync(newEmail);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ValidationException("Diese E-Mail-Adresse wird bereits verwendet.");
            }

            var setEmailResult = await userManager.SetEmailAsync(user, newEmail);
            ThrowIfFailed(setEmailResult);

            var setUserNameResult = await userManager.SetUserNameAsync(user, newEmail);
            ThrowIfFailed(setUserNameResult);

            user.EmailConfirmed = true;
        }

        user.FirstName = account.FirstName.Trim();
        user.LastName = account.LastName.Trim();
        user.WeeklyTargetHours = account.WeeklyTargetHours;
        user.AnnualVacationDays = account.AnnualVacationDays;
        user.CompanyName = account.CompanyName.Trim();
        user.TrainingOccupation = account.TrainingOccupation.Trim();
        user.TrainingYear = account.TrainingYear;

        var updateResult = await userManager.UpdateAsync(user);
        ThrowIfFailed(updateResult);
    }

    public async Task ChangePasswordAsync(
        ChangePasswordFormModel model,
        CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(model, new ValidationContext(model), validateAllProperties: true);

        var user = await currentUserService.GetRequiredUserAsync(cancellationToken);

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            var containsPasswordMismatch = result.Errors
                .Any(error => error.Code == "PasswordMismatch");

            if (containsPasswordMismatch)
            {
                throw new ValidationException("Das aktuelle Passwort ist nicht korrekt.");
            }

            ThrowIfFailed(result);
        }
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new ValidationException(string.IsNullOrWhiteSpace(errors)
                ? "Die Änderung konnte nicht gespeichert werden."
                : errors);
        }
    }
}
