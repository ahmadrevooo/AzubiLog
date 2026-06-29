using System.Text;
using AzubiLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AzubiLog.Services.Identity;

public sealed class AccountFlowService(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    DefaultUserData defaultUserData,
    IAccountEmailSender emailSender,
    ILogger<AccountFlowService> logger)
{
    public async Task<IdentityResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            WeeklyTargetHours = 40,
            AnnualVacationDays = 30,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return result;
        }

        await defaultUserData.EnsureDefaultCategoriesAsync(user.Id, cancellationToken);

        try
        {
            await SendConfirmationLinkAsync(user);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "E-Mail-Bestätigung konnte nicht gesendet werden für {Email}", user.Email);
        }

        return result;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = BuildAbsoluteLink($"/account/confirm-email?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(encodedToken)}");
        await emailSender.SendEmailConfirmationAsync(user, link);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var email = Uri.EscapeDataString(user.Email ?? string.Empty);
        var link = BuildAbsoluteLink($"/account/reset-password?email={email}&code={Uri.EscapeDataString(encodedToken)}");
        await emailSender.SendPasswordResetAsync(user, link);
    }

    public static string DecodeToken(string encodedToken)
    {
        var bytes = WebEncoders.Base64UrlDecode(encodedToken);
        return Encoding.UTF8.GetString(bytes);
    }

    private string BuildAbsoluteLink(string relativePathAndQuery)
    {
        var request = httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("A request is required to build account links.");

        return $"{request.Scheme}://{request.Host}{relativePathAndQuery}";
    }
}
