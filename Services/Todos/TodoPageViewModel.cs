namespace AzubiLog.Services.Todos;

public sealed class TodoPageViewModel
{
    public TodoFormModel NewTodo { get; init; } = new();
    public IReadOnlyList<TodoItemViewModel> OpenTodos { get; init; } = [];
    public IReadOnlyList<TodoItemViewModel> CompletedTodos { get; init; } = [];
}
