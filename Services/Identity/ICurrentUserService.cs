using AzubiLog.Models;

namespace AzubiLog.Services.Identity;

public interface ICurrentUserService
{
    Task<ApplicationUser> GetRequiredUserAsync(CancellationToken cancellationToken = default);
}
