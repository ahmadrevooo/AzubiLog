namespace AzubiLog.Services.Account;

public sealed class AccountOverviewViewModel
{
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string TrainingOccupation { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public int TrainingYear { get; set; } = 1;

    public AccountOverviewFormModel Account { get; set; } = new();
}
