using AzubiLog.Services.Profile;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages;

public partial class Settings : ComponentBase
{
    [Inject]
    private IApprenticeProfileService ProfileService { get; set; } = null!;

    protected ApprenticeProfileViewModel? ViewModel { get; private set; }
    protected ApprenticeProfileFormModel Profile { get; private set; } = new();
    protected string? StatusMessage { get; private set; }
    protected bool IsSaving { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        ViewModel = await ProfileService.GetProfileAsync();
        Profile = ViewModel.Profile;
    }

    protected async Task HandleSaveAsync()
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;
        StatusMessage = null;

        try
        {
            await ProfileService.SaveProfileAsync(Profile);
            ViewModel = await ProfileService.GetProfileAsync();
            Profile = ViewModel.Profile;
            StatusMessage = Localizer["ProfileSaved"];
        }
        catch (InvalidOperationException)
        {
            StatusMessage = Localizer["ProfileSaveFailed"];
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected void AddTrainer()
    {
        Profile.Trainers.Add(new TrainerFormModel());
    }

    protected void RemoveTrainer(TrainerFormModel trainer)
    {
        Profile.Trainers.Remove(trainer);
    }
}
