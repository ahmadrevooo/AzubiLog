namespace AzubiLog.Services.Pdf;

public interface IWeeklyReportPdfService
{
    /// <summary>
    /// Generates a printer-friendly PDF for the apprenticeship report week containing the specified date.
    /// </summary>
    /// <param name="date">Any date within the week that should be exported.</param>
    /// <param name="cancellationToken">Token used to cancel database access while preparing the PDF model.</param>
    /// <returns>The generated PDF file bytes.</returns>
    Task<byte[]> GenerateWeeklyReportPdfAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a stable German file name for the apprenticeship report week containing the specified date.
    /// </summary>
    /// <param name="date">Any date within the week that should be exported.</param>
    /// <returns>A PDF file name containing the ISO calendar week and year.</returns>
    string GetFileName(DateTime date);
}
