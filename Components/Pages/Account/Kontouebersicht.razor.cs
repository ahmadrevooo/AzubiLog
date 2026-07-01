using System.ComponentModel.DataAnnotations;
using AzubiLog.Services.Account;
using Microsoft.AspNetCore.Components;

namespace AzubiLog.Components.Pages.Account;

public partial class Kontouebersicht : ComponentBase
{
    [Inject]
    private IAccountOverviewService AccountService { get; set; } = null!;

    protected AccountOverviewViewModel? ViewModel { get; private set; }
    protected AccountOverviewFormModel Account { get; private set; } = new();
    protected ChangePasswordFormModel Password { get; private set; } = new();

    protected string? AccountStatus { get; private set; }
    protected bool AccountHasError { get; private set; }
    protected bool IsSavingAccount { get; private set; }

    protected string? PasswordStatus { get; private set; }
    protected bool PasswordHasError { get; private set; }
    protected bool IsChangingPassword { get; private set; }

    protected string Initials => BuildInitials();

    protected override async Task OnInitializedAsync()
    {
        ViewModel = await AccountService.GetAccountAsync();
        Account = ViewModel.Account;
    }

    protected async Task HandleSaveAccountAsync()
    {
        if (IsSavingAccount)
        {
            return;
        }

        IsSavingAccount = true;
        AccountStatus = null;
        AccountHasError = false;

        try
        {
            await AccountService.SaveAccountAsync(Account);
            ViewModel = await AccountService.GetAccountAsync();
            Account = ViewModel.Account;
            AccountStatus = "Gespeichert.";
        }
        catch (ValidationException ex)
        {
            AccountHasError = true;
            AccountStatus = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            AccountHasError = true;
            AccountStatus = ex.Message;
        }
        finally
        {
            IsSavingAccount = false;
        }
    }

    protected async Task HandleChangePasswordAsync()
    {
        if (IsChangingPassword)
        {
            return;
        }

        IsChangingPassword = true;
        PasswordStatus = null;
        PasswordHasError = false;

        try
        {
            await AccountService.ChangePasswordAsync(Password);
            Password = new ChangePasswordFormModel();
            PasswordStatus = "Passwort geändert.";
        }
        catch (ValidationException ex)
        {
            PasswordHasError = true;
            PasswordStatus = ex.Message;
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    private string BuildInitials()
    {
        if (ViewModel is null)
        {
            return "?";
        }

        var first = Account.FirstName?.Trim();
        var last = Account.LastName?.Trim();

        var initials = string.Concat(
            string.IsNullOrEmpty(first) ? string.Empty : first[..1],
            string.IsNullOrEmpty(last) ? string.Empty : last[..1]);

        if (!string.IsNullOrEmpty(initials))
        {
            return initials.ToUpperInvariant();
        }

        var email = ViewModel.Email;
        return string.IsNullOrEmpty(email) ? "?" : email[..1].ToUpperInvariant();
    }
}
