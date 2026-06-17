namespace AzubiLog.Services.Profile;

public sealed class ApprenticeProfileViewModel
{
    public string Email { get; init; } = string.Empty;
    public ApprenticeProfileFormModel Profile { get; init; } = new();
}
