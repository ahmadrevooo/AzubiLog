namespace AzubiLog.Services.ReportEntries;

public interface IReportEntryService
{
    Task<ReportEntryEditorViewModel> GetEditorAsync(int? entryId, DateTime? date, CancellationToken cancellationToken = default);
    Task<ReportEntryEditorViewModel> RefreshEditorAsync(ReportEntryFormModel form, CancellationToken cancellationToken = default);
    Task<ReportEntryFormModel> SaveDraftAsync(ReportEntryFormModel form, CancellationToken cancellationToken = default);
    Task<int> SaveEntryAsync(ReportEntryFormModel form, CancellationToken cancellationToken = default);
    Task DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default);
    Task<int> CreateCategoryAsync(string name, CancellationToken cancellationToken = default);
    Task<WeeklyOverviewViewModel> GetWeeklyOverviewAsync(DateTime date, CancellationToken cancellationToken = default);
    decimal CalculateHours(string startTime, string endTime);
}
