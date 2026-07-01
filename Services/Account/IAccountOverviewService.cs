namespace AzubiLog.Services.Account;

public interface IAccountOverviewService
{
    Task<AccountOverviewViewModel> GetAccountAsync(CancellationToken cancellationToken = default);

    Task SaveAccountAsync(AccountOverviewFormModel account, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(ChangePasswordFormModel model, CancellationToken cancellationToken = default);
}
