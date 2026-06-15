namespace AzubiLog.Interfaces;

/// <summary>Manages user-owned report categories.</summary>
public interface ICategoryService
{
    /// <summary>Creates any missing default categories for a newly registered user.</summary>
    /// <param name="userId">The Identity user identifier.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task EnsureDefaultsAsync(string userId, CancellationToken cancellationToken = default);
}
