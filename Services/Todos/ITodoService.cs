namespace AzubiLog.Services.Todos;

public interface ITodoService
{
    Task<TodoPageViewModel> GetTodoPageAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItemViewModel>> GetOpenTodosAsync(int take, CancellationToken cancellationToken = default);
    Task<int> GetOpenTodoCountAsync(CancellationToken cancellationToken = default);
    Task CreateTodoAsync(TodoFormModel form, CancellationToken cancellationToken = default);
    Task CompleteTodoAsync(int todoId, CancellationToken cancellationToken = default);
    Task ReopenTodoAsync(int todoId, CancellationToken cancellationToken = default);
    Task DeleteTodoAsync(int todoId, CancellationToken cancellationToken = default);
}
