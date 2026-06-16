using AzubiLog.Models;

namespace AzubiLog.Services.Identity;

public sealed class DevelopmentAccountEmailSender(
    ILogger<DevelopmentAccountEmailSender> logger,
    IWebHostEnvironment environment) : IAccountEmailSender
{
    public Task SendEmailConfirmationAsync(ApplicationUser user, string confirmationLink)
    {
        if (environment.IsDevelopment())
        {
            logger.LogWarning("AzubiLog email confirmation link for {Email}: {ConfirmationLink}", user.Email, confirmationLink);
        }

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(ApplicationUser user, string resetLink)
    {
        if (environment.IsDevelopment())
        {
            logger.LogWarning("AzubiLog password reset link for {Email}: {ResetLink}", user.Email, resetLink);
        }

        return Task.CompletedTask;
    }
}
