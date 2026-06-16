namespace AzubiLog.Services.Todos;

public sealed record TodoItemViewModel(
    int Id,
    string Title,
    string Description,
    DateTime? DueDate,
    bool IsCompleted);
