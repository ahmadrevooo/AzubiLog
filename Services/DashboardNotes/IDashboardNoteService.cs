namespace AzubiLog.Services.DashboardNotes;

public interface IDashboardNoteService
{
    Task<IReadOnlyList<DashboardNoteViewModel>> GetRecentNotesAsync(int take, CancellationToken cancellationToken = default);
    Task CreateNoteAsync(DashboardNoteFormModel form, CancellationToken cancellationToken = default);
}
