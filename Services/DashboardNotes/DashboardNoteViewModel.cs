namespace AzubiLog.Services.DashboardNotes;

public sealed record DashboardNoteViewModel(
    int Id,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
