using AzubiLog.Models;

namespace AzubiLog.Services.Identity;

public interface IAccountEmailSender
{
    Task SendEmailConfirmationAsync(ApplicationUser user, string confirmationLink);
    Task SendPasswordResetAsync(ApplicationUser user, string resetLink);
}
