namespace AzubiLog.Services.Profile;

public interface IApprenticeProfileService
{
    Task<ApprenticeProfileViewModel> GetProfileAsync(CancellationToken cancellationToken = default);
    Task SaveProfileAsync(ApprenticeProfileFormModel profile, CancellationToken cancellationToken = default);
}
